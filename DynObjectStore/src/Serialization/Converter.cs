// namespace DynObjectStore;

// using Newtonsoft.Json;
// using Newtonsoft.Json.Linq;
// using System.Collections;
// using System.Reflection;
// using System.Runtime.Serialization;


// public class ReferenceJsonConverter(ObjectReferences refs)
// {
//     public HashSet<int> idsWritten = new HashSet<int>();

//     public void WriteJson(JsonWriter writer, object? value)
//     {
//         if (value == null)
//         {
//             writer.WriteNull();
//             return;
//         }

//         var type = value.GetType();

//         // Value types and primitives are serialized normally
//         if (type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal))
//         {
//             writer.WriteValue(value);
//             return;
//         }

//         // Handle references
//         int id = refs.registerSubObject(value);
//         if (idsWritten.Contains(id))
//         {
//             // Already serialized: write a ref
//             writer.WriteStartObject();
//             writer.WritePropertyName("$ref");
//             writer.WriteValue(id);
//             writer.WriteEndObject();
//             return;
//         }

//         // Write object
//         writer.WriteStartObject();

//         // Always write the reference id
//         writer.WritePropertyName("$id");
//         writer.WriteValue(id);
//         idsWritten.Add(id);

//         // Write type info (optional but helps for polymorphic)
//         writer.WritePropertyName("$type");
//         writer.WriteValue(type.AssemblyQualifiedName);

//         // Write attachments
//         var attachments = refs.getAttachements(value);
//         if (attachments != null && attachments.Count > 0)
//         {
//             writer.WritePropertyName("$attachments");
//             writer.WriteStartObject();
//             foreach (var kv in attachments)
//             {
//                 writer.WritePropertyName(kv.Key);
//                 WriteJson(writer, kv.Value);
//             }
//             writer.WriteEndObject();
//         }

//         // Handle collections
//         if (typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string))
//         {
//             writer.WritePropertyName("$values");
//             writer.WriteStartArray();
//             foreach (var item in (IEnumerable)value)
//             {
//                 WriteJson(writer, item);
//             }
//             writer.WriteEndArray();
//         }
//         else
//         {
//             // Handle fields and properties (public + private)
//             var members = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
//             foreach (var field in members)
//             {
//                 writer.WritePropertyName(field.Name);
//                 WriteJson(writer, field.GetValue(value));
//             }

//             var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
//             foreach (var prop in props)
//             {
//                 if (!prop.CanRead) continue;
//                 writer.WritePropertyName(prop.Name);
//                 WriteJson(writer, prop.GetValue(value));
//             }
//         }

//         writer.WriteEndObject();
//     }

//     public object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
//     {
//         if (reader.TokenType == JsonToken.Null)
//             return null;

//         var jObj = JObject.Load(reader);

//         // Handle $ref
//         if (jObj["$ref"] != null)
//         {
//             int refId = jObj["$ref"]!.Value<int>();
//             return refs.getSubObject(refId);
//         }

//         // Resolve type (polymorphic support)
//         Type actualType = objectType;
//         if (jObj["$type"] != null)
//         {
//             actualType = Type.GetType(jObj["$type"]!.Value<string>()) ?? objectType;
//         }

//         // Create instance (bypassing constructor if needed)
//         object instance = FormatterServices.GetUninitializedObject(actualType);
//         int id = jObj["$id"]?.Value<int>() ?? refs.nextId;
//         refs.registerSubObject(instance, id);

//         // Load attachments
//         var attachmentsToken = jObj["$attachments"];
//         if (attachmentsToken != null && attachmentsToken.Type == JTokenType.Object)
//         {
//             var dict = new Dictionary<string, object>();
//             foreach (var prop in (JObject)attachmentsToken)
//             {
//                 dict[prop.Key] = prop.Value!.ToObject<object>(serializer)!;
//             }
//             refs.setAttachements(instance, dict);
//         }

//         // Handle collections
//         if (typeof(IEnumerable).IsAssignableFrom(actualType) && actualType != typeof(string))
//         {
//             var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(actualType.GetGenericArguments()[0]))!;
//             foreach (var itemToken in jObj["$values"] ?? new JArray())
//             {
//                 list.Add(itemToken.ToObject(actualType.GetGenericArguments()[0], serializer)!);
//             }
//             return list;
//         }

//         // Set fields
//         foreach (var field in actualType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
//         {
//             if (jObj[field.Name] != null)
//             {
//                 var val = jObj[field.Name]!.ToObject(field.FieldType, serializer);
//                 field.SetValue(instance, val);
//             }
//         }

//         // Set properties
//         foreach (var prop in actualType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
//         {
//             if (!prop.CanWrite) continue;
//             if (jObj[prop.Name] != null)
//             {
//                 var val = jObj[prop.Name]!.ToObject(prop.PropertyType, serializer);
//                 prop.SetValue(instance, val);
//             }
//         }

//         return instance;
//     }
// }
