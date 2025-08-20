using DynObjectStore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

public class AttachmentsContractResolver(Registry registry, ObjectReferences objRef) : DefaultContractResolver
{
    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    {
        var props = base.CreateProperties(type, memberSerialization);

        // Add our special "__Attachments__" property
        props.Add(new JsonProperty
        {
            PropertyName = "__Attachments__",
            PropertyType = typeof(List<Dictionary<string, object>>),
            Readable = true,
            Writable = true,
            ValueProvider = new AttachmentsValueProvider(registry, objRef),
            DeclaringType = type
        });

        return props;
    }
}

public class AttachmentsValueProvider(Registry registry, ObjectReferences objRef) : IValueProvider
{

    public object? GetValue(object target)
    {
        Dictionary<string, object>? dict = objRef.getAttachements(target);
        if (dict == null) return null;
        return dict.ToDictionary(x => x.Key, x => registry.ObjectIds[x.Value]);
    }

    public void SetValue(object target, object? value)
    {
        if (value is Dictionary<string, int> dict)
        {
            objRef.setAttachements(target, dict.ToDictionary(x => x.Key, x => registry.Objects[x.Value]));
        }
    }
}
