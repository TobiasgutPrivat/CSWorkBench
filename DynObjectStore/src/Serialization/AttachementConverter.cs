using System.Reflection;
using DynObjectStore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class AttachmentsConverter(Registry registry, ObjectReferences objRef) : JsonConverter
{
    public override bool CanConvert(Type objectType) => true;

    public override bool CanWrite => false;

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) => throw new NotImplementedException();

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        // Load the JSON object
        JObject jo = JObject.Load(reader);

        // Create an instance without using serializer (avoids recursion)
        object target = Activator.CreateInstance(objectType)!;

        // Populate normal properties
        foreach (var prop in objectType.GetProperties(BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            if (!prop.CanWrite) continue;
            if (jo.TryGetValue(prop.Name, out var token))
            {
                object? value = token.ToObject(prop.PropertyType, serializer);
                prop.SetValue(target, value);
            }
        }

        // Handle attachments manually
        if (jo.TryGetValue("__Attachments__", out var attToken))
        {
            var dict = attToken.ToObject<Dictionary<string, int>>();
            if (dict != null)
            {
                objRef.setAttachements(target, dict.ToDictionary(x => x.Key, x => registry.Objects[x.Value]));
            }
        }

        return target;
    }
}
