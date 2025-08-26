using System.Collections;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DynObjectStore;

public class Deserializer(Registry registry, ObjectReferences refs)
{
    public object? Deserialize(string json, Type type)
    {
        using var sr = new StringReader(json);
        using var reader = new JsonTextReader(sr);
        return ReadJson(reader, type);
    }

    public object? ReadJson(JsonReader reader, Type objectType)
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
            return refs.getSubObject(refId);
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
        var ctor = actualType.GetConstructor(Type.EmptyTypes);
        if (ctor != null)
        {
            instance = ctor.Invoke(null);
        }
        else
        {
            instance = FormatterServices.GetUninitializedObject(actualType);
        }
        int id = jToken["$id"]?.Value<int>() ?? refs.nextId;
        refs.registerSubObject(instance, id);

        // Load attachments
        var attachmentsToken = jToken["$attachments"];
        if (attachmentsToken != null && attachmentsToken.Type == JTokenType.Object)
        {
            Dictionary<string, int> dict = new Dictionary<string, int>();
            foreach (var prop in (JObject)attachmentsToken)
            {
                dict[prop.Key] = prop.Value!.Value<int>();
            }
            refs.setAttachements(instance, dict.ToDictionary(x => x.Key, x => registry.GetObject(x.Value)));
        }

        // Handle dictionaries
        if (typeof(IDictionary).IsAssignableFrom(actualType))
        {
            var dict = (IDictionary)(Activator.CreateInstance(actualType)
                         ?? FormatterServices.GetUninitializedObject(actualType));
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

                IList list;
                if (typeof(IList).IsAssignableFrom(actualType) && !actualType.IsInterface && !actualType.IsAbstract)
                {
                    list = (IList)(Activator.CreateInstance(actualType)
                           ?? FormatterServices.GetUninitializedObject(actualType));
                }
                else
                {
                    list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType))!;
                }

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