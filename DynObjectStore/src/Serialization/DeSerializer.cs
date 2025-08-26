using System.Collections;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DynObjectStore;

public class Deserializer(ObjectReferences refs)
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
            var dict = new Dictionary<string, object>();
            foreach (var prop in (JObject)attachmentsToken)
            {
                dict[prop.Key] = ReadJson(prop.Value.CreateReader(), typeof(object))!;
            }
            refs.setAttachements(instance, dict);
        }

        // Handle collections
        if (typeof(IEnumerable).IsAssignableFrom(actualType) && actualType != typeof(string))
        {
            var valuesToken = jToken["$values"];
            if (valuesToken != null && typeof(IList).IsAssignableFrom(actualType))
            {
                var list = (IList)Activator.CreateInstance(actualType)!;
                foreach (var itemToken in valuesToken)
                {
                    var elementType = actualType.IsGenericType ? actualType.GetGenericArguments()[0] : typeof(object);
                    list.Add(ReadJson(itemToken.CreateReader(), elementType)!);
                }
                return list;
            }
        }

        // Populate fields
        foreach (var field in actualType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            var token = jToken[field.Name];
            if (token != null)
            {
                field.SetValue(instance, ReadJson(token.CreateReader(), field.FieldType));
            }
        }

        // Populate properties
        foreach (var prop in actualType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            if (!prop.CanWrite) continue;
            var token = jToken[prop.Name];
            if (token != null)
            {
                prop.SetValue(instance, ReadJson(token.CreateReader(), prop.PropertyType));
            }
        }

        return instance;
    }
}
