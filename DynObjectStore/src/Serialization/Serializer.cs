namespace DynObjectStore;

using System.Collections;
using System.Reflection;
using Newtonsoft.Json;

internal class Serializer(RootObject rootObject)
{
    internal string Serialize(object obj)
    {
        using var sw = new StringWriter();
        using var writer = new JsonTextWriter(sw)
        {
            Formatting = Formatting.Indented
        };

        WriteJson(writer, obj);
        writer.Flush();

        return sw.ToString();
    }

    internal HashSet<int> idsWritten = new();

    internal void WriteJson(JsonWriter writer, object? value)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        var type = value.GetType();

        // Value types and primitives are serialized normally
        if (type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal))
        {
            writer.WriteValue(value);
            return;
        }

        // Structs (value types) should serialize inline, not by ref
        if (type.IsValueType)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("$type");
            writer.WriteValue(type.AssemblyQualifiedName);


            foreach (FieldInfo field in ReflectionCache.GetSerializableFields(type))
            {
                writer.WritePropertyName(field.Name);
                WriteJson(writer, field.GetValue(value));
            }

            writer.WriteEndObject();
            return;
        }

        // Handle references for reference types
        int id = rootObject.registerSubObject(value);
        if (idsWritten.Contains(id))
        {
            // Already serialized: write a ref
            writer.WriteStartObject();
            writer.WritePropertyName("$ref");
            writer.WriteValue(id);
            writer.WriteEndObject();
            return;
        }

        // Write object
        writer.WriteStartObject();

        // Always write the reference id
        writer.WritePropertyName("$id");
        writer.WriteValue(id);
        idsWritten.Add(id);

        // Write type info
        writer.WritePropertyName("$type");
        writer.WriteValue(type.AssemblyQualifiedName);

        // Write attachments
        var attachments = rootObject.getAttachements(value);
        if (attachments != null && attachments.Count > 0)
        {
            writer.WritePropertyName("$attachments");
            writer.WriteStartObject();
            foreach (var attachement in attachments)
            {
                writer.WritePropertyName(attachement.Key);
                WriteJson(writer, attachement.Value.id);
            }
            writer.WriteEndObject();
        }

        // Handle collections (ensure $id written before $values)
        if (typeof(IDictionary).IsAssignableFrom(type))
        {
            writer.WritePropertyName("$values");
            writer.WriteStartArray();
            foreach (DictionaryEntry kv in (IDictionary)value)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("Key");
                WriteJson(writer, kv.Key);
                writer.WritePropertyName("Value");
                WriteJson(writer, kv.Value);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
        }
        else if (typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string))
        {
            writer.WritePropertyName("$values");
            writer.WriteStartArray();
            foreach (var item in (IEnumerable)value)
            {
                WriteJson(writer, item);
            }
            writer.WriteEndArray();
        }
        else
        {
            // handle fields (only fields because: https://chatgpt.com/share/68ad9143-a9e4-8007-b614-bd744eeeb8c0)
            List<FieldInfo> fields = ReflectionCache.GetSerializableFields(type);

            foreach (var field in fields)
            {
                var val = field.GetValue(value);
                writer.WritePropertyName(field.Name);
                WriteJson(writer, val);
            }
        }

        writer.WriteEndObject();
    }
}