using DynObjectStore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

public class AttachmentsContractResolver(Registry registry, ObjectReferences objRef) : DefaultContractResolver
{
    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    {
        var props = base.CreateProperties(type, memberSerialization);

        var attachmentsProp = new JsonProperty
        {
            PropertyName = "__Attachments__",
            PropertyType = typeof(object),
            Readable = true,
            Writable = true,
            ValueProvider = new AttachmentsValueProvider(registry, objRef),
            DeclaringType = type,
            HasMemberAttribute = true,
            UnderlyingName = "__Attachments__"
        };

        // Only serialize if there's actual content
        attachmentsProp.ShouldSerialize = instance =>
        {
            var value = attachmentsProp.ValueProvider.GetValue(instance) as Dictionary<string, int>;
            return value != null && value.Count > 0;
        };

        props.Add(attachmentsProp);

        return props;
    }
}

public class AttachmentsValueProvider(Registry registry, ObjectReferences objRef) : IValueProvider
{

    public object? GetValue(object target)
    {
        Dictionary<string, object>? attachements = objRef.getAttachements(target);
        if (attachements == null) return null;
        Dictionary<string, int> attachementIds = attachements.ToDictionary(x => x.Key, x => registry.ObjectIds[x.Value]);
        return attachementIds;
    }

    public void SetValue(object target, object? value)
    {
        Console.WriteLine($"SetValue called for {target.GetType().Name}");

        if (value is JObject jObj)
        {
            var dict = jObj.ToObject<Dictionary<string, int>>();
            objRef.setAttachements(target, dict!.ToDictionary(x => x.Key, x => registry.Objects[x.Value]));
        }
    }
}
