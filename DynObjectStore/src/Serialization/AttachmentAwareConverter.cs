// using System.Reflection;
// using Newtonsoft.Json;
// using Newtonsoft.Json.Linq;

// namespace DynObjectStore;

// public class AttachmentAwareConverter(Registry registry, ObjectReferences objRef) : JsonConverter
// {

//     public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
//     {
//         // default behaviour
//         if (value == null)
//         {
//             writer.WriteNull();
//             return;
//         }

//         Type type = value.GetType();
//         writer.WriteStartObject();

//         // Write all public properties
//         foreach (PropertyInfo prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
//         {
//             if (!prop.CanRead) continue;

//             object propValue = prop.GetValue(value);
//             writer.WritePropertyName(prop.Name);

//             // Serialize the property value normally (nested objects hit this converter)
//             serializer.Serialize(writer, propValue);
//         }

//         // handle attachements
//         if (objRef.Attachements.TryGetValue(value, out var attachments))
//         {
//             var attList = new JArray();
//             foreach (var entry in attachments)
//             {
//                 attList.Add(new JObject
//                 {
//                     ["name"] = entry.Key.ToString(),
//                     ["id"] = registry.ObjectIds[entry.Value] // root object id
//                 });
//             }
//             writer.WritePropertyName("#attachments");
//             writer.WriteValue(attList);
//         }

//         writer.WriteEndObject();
//     }

//     public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
//     {
//         if (reader.TokenType == JsonToken.Null)
//             return null;

//         object instance = Activator.CreateInstance(objectType);

//         // Load the object as JObject so we can inspect special properties
//         var jo = JObject.Load(reader);

//         // Handle #attachments first
//         if (jo.TryGetValue("#attachments", out JToken? attToken))
//         {
//             foreach (var att in attToken.Children<JObject>())
//             {
//                 string name = att["name"]!.ToString();
//                 int id = att["id"]!.ToObject<int>()!;

//                 // store in Registry instead of assigning to result
//                 if (!objRef.Attachements.TryGetValue(instance, out var attachments))
//                 {
//                     attachments = new Dictionary<string, object>();
//                     objRef.Attachements[instance] = attachments;
//                 }

//                 attachments[name] = registry.GetObject(id);
//             }

//             // Remove the special property so we don't try to set it
//             jo.Remove("#attachments");
//         }

//         // Deserialize normal properties
//         foreach (var prop in objectType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
//         {
//             if (!prop.CanWrite) continue;

//             if (jo.TryGetValue(prop.Name, out JToken? token))
//             {
//                 object? value = token.ToObject(prop.PropertyType, serializer);
//                 prop.SetValue(instance, value);
//             }
//         }

//         return instance;
//     }

//     public override bool CanConvert(Type objectType)
//     {
//         // Apply to all reference types except primitives and strings
//         return !objectType.IsPrimitive && objectType != typeof(string);
//     }
// }
