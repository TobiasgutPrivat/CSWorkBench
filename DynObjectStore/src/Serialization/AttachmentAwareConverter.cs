using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DynObjectStore;

public class AttachmentAwareConverter<T>(Registry registry, ObjectReferences objRef) : JsonConverter<T>
{

    public override void WriteJson(JsonWriter writer, T? value, JsonSerializer serializer)
    {
        JObject obj = JObject.FromObject(value!, serializer);

        if (objRef.Attachements.TryGetValue(value, out var attachments))
        {
            var attList = new JArray();
            foreach (var entry in attachments)
            {
                attList.Add(new JObject
                {
                    ["name"] = entry.Key.ToString(),
                    ["id"] = registry.ObjectIds[entry.Value] // root object id
                });
            }
            obj["#attachments"] = attList;
        }

        obj.WriteTo(writer);
    }

    public override T? ReadJson(JsonReader reader, Type objectType, T? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        JObject obj = JObject.Load(reader);

        // normal properties
        T result = obj.ToObject<T>(serializer)!;

        // extract attachments
        if (obj.TryGetValue("#attachments", out JToken? attToken))
        {
            foreach (var att in attToken.Children<JObject>())
            {
                string name = att["name"]!.ToString();
                int id = att["id"]!.ToObject<int>();

                // store in Registry instead of assigning to result
                if (objRef.Attachements.TryGetValue(result, out var attachments))
                {
                    attachments[name] = registry.GetObject(id);
                }
                else
                {
                    attachments = new Dictionary<string, object>();
                    attachments[name] = registry.GetObject(id);
                    objRef.Attachements[result] = attachments;
                }
            }
        }

        return result;
    }

    public override bool CanRead => true;
    public override bool CanWrite => true;
}
