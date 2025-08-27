using System.Collections;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DynObjectStore;

internal class Deserializer(RootObject rootObject)
{
    internal object? Deserialize(string json, Type type)
    {
        using var sr = new StringReader(json);
        using var reader = new JsonTextReader(sr);
        return ReadJson(reader, type);
    }

    internal object? ReadJson(JsonReader reader, Type objectType)
    {
        if (reader.TokenType == JsonToken.Null)
            return null;

        var jToken = JToken.Load(reader);

        // Handle primitives
        if (objectType.IsPrimitive || objectType.IsEnum || objectType == typeof(string) || objectType == typeof(decimal))
        {
            return jToken.ToObject(objectType);
        }

        // Handle $ref
        if (jToken["$ref"] != null)
        {
            int refId = jToken["$ref"]!.Value<int>();
            return rootObject.getSubObject(refId);
        }

        // Resolve type
        Type actualType = objectType;
        if (jToken["$type"] != null)
        {
            actualType = Type.GetType(jToken["$type"]!.Value<string>()) ?? objectType;
        }

        // Value types (structs) → just populate
        if (actualType.IsValueType)
        {
            object structInstance = FormatterServices.GetUninitializedObject(actualType);

            foreach (var field in actualType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var token = jToken[field.Name];
                if (token != null)
                {
                    field.SetValueDirect(__makeref(structInstance),
                        ReadJson(token.CreateReader(), field.FieldType));
                }
            }

            return structInstance;
        }

        // Create uninitialized instance
        object instance;
        if (actualType.IsArray)
        {
            // For arrays we defer actual creation until we know element count
            instance = null!;
        }
        else
        {
            var ctor = actualType.GetConstructor(Type.EmptyTypes);
            if (ctor != null)
            {
                instance = ctor.Invoke(null);
            }
            else
            {
                instance = FormatterServices.GetUninitializedObject(actualType);
            }

            // Register immediately so $ref works and attachments can bind
        }

        int id = jToken["$id"]?.Value<int>() ?? rootObject.nextId;
        if (instance != null) // arrays will be handled after creation
            rootObject.registerSubObject(instance, id);

        // Load attachments
        var attachmentsToken = jToken["$attachments"];
        if (attachmentsToken != null && attachmentsToken.Type == JTokenType.Object)
        {
            Dictionary<string, int> dict = new Dictionary<string, int>();
            foreach (var prop in (JObject)attachmentsToken)
            {
                dict[prop.Key] = prop.Value!.Value<int>();
            }
            rootObject.setAttachements(instance, dict.ToDictionary(x => x.Key, x => rootObject.registry.GetObject(x.Value)));
        }

        // Handle arrays
        if (actualType.IsArray)
        {
            var elementType = actualType.GetElementType() ?? typeof(object);
            var valuesToken = jToken["$values"];
            if (valuesToken != null)
            {
                var tempList = new List<object?>();
                foreach (var itemToken in valuesToken)
                {
                    tempList.Add(ReadJson(itemToken.CreateReader(), elementType));
                }

                var array = Array.CreateInstance(elementType, tempList.Count);
                for (int i = 0; i < tempList.Count; i++)
                {
                    array.SetValue(tempList[i], i);
                }

                // Register the actual array now (replacing the placeholder)
                rootObject.registerSubObject(array, id);
                return array;
            }
            return Array.CreateInstance(elementType, 0);
        }

        // Handle dictionaries
        if (typeof(IDictionary).IsAssignableFrom(actualType))
        {
            var dict = (IDictionary)instance;
            var valuesToken = jToken["$values"];
            if (valuesToken != null)
            {
                foreach (var kvToken in valuesToken)
                {
                    var key = ReadJson(kvToken["Key"]!.CreateReader(), typeof(object));
                    var val = ReadJson(kvToken["Value"]!.CreateReader(), typeof(object));
                    dict.Add(key!, val);
                }
            }
            return dict;
        }

        // Handle lists/collections
        if (typeof(IEnumerable).IsAssignableFrom(actualType) && actualType != typeof(string))
        {
            var valuesToken = jToken["$values"];
            if (valuesToken != null)
            {
                var elementType = actualType.IsGenericType ? actualType.GetGenericArguments()[0] : typeof(object);

                IList list = instance as IList;

                foreach (var itemToken in valuesToken)
                {
                    list.Add(ReadJson(itemToken.CreateReader(), elementType)!);
                }
                return list;
            }
        }

        // Populate fields
        Type? currtype = actualType;
        List<FieldInfo> fields = new List<FieldInfo>();
        while (currtype != null)
        {
            fields.AddRange(currtype.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly));
            currtype = currtype.BaseType;
        }

        foreach (var field in fields)
        {
            var token = jToken[field.Name];
            if (token != null)
            {
                field.SetValue(instance, ReadJson(token.CreateReader(), field.FieldType));
            }
        }

        return instance;
    }
}