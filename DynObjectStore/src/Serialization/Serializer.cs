using System.Collections;
using System.Reflection;
using Newtonsoft.Json;

namespace DynObjectStore;

public class Serializer(ObjectReferences refs)
{
    public string Serialize(object obj)
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

    public HashSet<int> idsWritten = new HashSet<int>();

    public void WriteJson(JsonWriter writer, object? value)
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

        // Handle references
        int id = refs.registerSubObject(value);
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

        // Write type info (optional but helps for polymorphic)
        writer.WritePropertyName("$type");
        writer.WriteValue(type.AssemblyQualifiedName);

        // Write attachments
        var attachments = refs.getAttachements(value);
        if (attachments != null && attachments.Count > 0)
        {
            writer.WritePropertyName("$attachments");
            writer.WriteStartObject();
            foreach (var kv in attachments)
            {
                writer.WritePropertyName(kv.Key);
                WriteJson(writer, kv.Value);
            }
            writer.WriteEndObject();
        }

        // Handle collections
        if (typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string))
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
            // Handle fields and properties (public + private)
            var members = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in members)
            {
                writer.WritePropertyName(field.Name);
                WriteJson(writer, field.GetValue(value));
            }

            var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var prop in props)
            {
                if (!prop.CanRead) continue;
                writer.WritePropertyName(prop.Name);
                WriteJson(writer, prop.GetValue(value));
            }
        }

        writer.WriteEndObject();
    }
}
