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
        using StringReader sr = new StringReader(json);
        using JsonTextReader reader = new JsonTextReader(sr);
        return ReadJson(reader, type);
    }

    internal object? ReadJson(JsonReader reader, Type objectType)
    {
        if (reader.TokenType == JsonToken.Null)
            return null;

        JToken jToken = JToken.Load(reader);

        // Handle primitives
        if (objectType.IsPrimitive || objectType.IsEnum || objectType == typeof(string) || objectType == typeof(decimal))
        {
            return jToken.ToObject(objectType);
        }

        // Handle $ref
        if (jToken["$ref"] != null)
        {
            int refId = jToken["$ref"]!.Value<int>();
            return rootObject.GetSubObject(refId);
        }

        // Resolve type
        Type actualType = objectType;
        if (jToken["$type"] != null)
        {
            actualType = Type.GetType(jToken["$type"]!.Value<string>()!) ?? objectType;
        }

        // Value types (structs) → just populate
        if (actualType.IsValueType)
        {
            object structInstance = FormatterServices.GetUninitializedObject(actualType);

            foreach (FieldInfo field in ReflectionCache.GetSerializableFields(actualType))
            {
                JToken? token = jToken[field.Name];
                if (token != null)
                {
                    object? fieldValue = ReadJson(token.CreateReader(), field.FieldType);
                    if (fieldValue != null)
                    {
                        field.SetValueDirect(__makeref(structInstance), fieldValue);
                    }
                }
            }

            return structInstance;
        }

        // read metadata
        int id = jToken["$id"]?.Value<int>() ?? rootObject.nextId;
        JToken? attachmentsToken = jToken["$attachments"];
        Dictionary<string, RootObject>? attachements = null;
        if (attachmentsToken != null && attachmentsToken.Type == JTokenType.Object)
        {
            Dictionary<string, int> dict = new Dictionary<string, int>();
            foreach (var prop in (JObject)attachmentsToken)
            {
                dict[prop.Key] = prop.Value!.Value<int>();
            }

            attachements = dict.Where(x => rootObject.registry.GetObject(x.Value) != null)
                .ToDictionary(x => x.Key, x => rootObject.registry.GetObject(x.Value)!);

            if (attachements.Count == 0) attachements = null;
        }

        // Handle arrays
        if (actualType.IsArray)
        {
            Type elementType = actualType.GetElementType() ?? typeof(object);
            JArray? valuesToken = jToken["$values"] as JArray;
            int count = valuesToken?.Count ?? 0;

            // Create and register the array immediately so $ref to this array works.
            Array array = Array.CreateInstance(elementType, count);

            // Use the provided $id if present; otherwise use nextId as you already do.
            rootObject.RegisterSubObject(array, id);
            rootObject.SetAttachements(array, attachements);

            // Now populate the elements
            if (valuesToken != null)
            {
                for (int i = 0; i < count; i++)
                {
                    object? item = ReadJson(valuesToken[i].CreateReader(), elementType);
                    array.SetValue(item, i);
                }
            }

            return array;
        }

        // Create uninitialized instance
        object instance = null!;
        ConstructorInfo? ctor = actualType.GetConstructor(Type.EmptyTypes);
        if (ctor != null)
        {
            instance = ctor.Invoke(null);
        }
        else
        {
            instance = FormatterServices.GetUninitializedObject(actualType);
        }

        rootObject.RegisterSubObject(instance, id);
        rootObject.SetAttachements(instance, attachements);

        // Handle dictionaries
        if (instance as IDictionary != null)
        {
            IDictionary dict = (IDictionary)instance;
            JToken? valuesToken = jToken["$values"];
            if (valuesToken != null)
            {
                foreach (JToken kvToken in valuesToken)
                {
                    object? key = ReadJson(kvToken["Key"]!.CreateReader(), typeof(object));
                    object? val = ReadJson(kvToken["Value"]!.CreateReader(), typeof(object));
                    dict.Add(key!, val);
                }
            }
            return dict;
        }

        // Handle lists/collections
        if (instance as IEnumerable != null && actualType != typeof(string))
        {
            JToken? valuesToken = jToken["$values"];
            if (valuesToken != null)
            {
                Type elementType = actualType.IsGenericType ? actualType.GetGenericArguments()[0] : typeof(object);

                IList list = instance as IList ?? throw new NotImplementedException(); //TODO check this case
                foreach (JToken itemToken in valuesToken)
                {
                    list.Add(ReadJson(itemToken.CreateReader(), elementType)!);
                }
                return list;
            }
        }

        // Populate fields
        List<FieldInfo> fields = ReflectionCache.GetSerializableFields(actualType);

        MemberInfo[] memberInfos = fields.Cast<MemberInfo>().ToArray();
        object?[] memberValues = new object?[memberInfos.Length];

        for (int i = 0; i < memberInfos.Length; i++)
        {
            FieldInfo f = (FieldInfo)memberInfos[i];
            JToken? token = jToken[f.Name];
            memberValues[i] = token != null ? ReadJson(token.CreateReader(), f.FieldType) : null;
        }

        // This sets even private/readonly fields in a supported way
        FormatterServices.PopulateObjectMembers(instance, memberInfos, memberValues);

        return instance;
    }
}