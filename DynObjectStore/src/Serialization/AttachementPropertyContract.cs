using DynObjectStore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;

public class AttachmentsContractResolver(ObjectReferences objRef) : DefaultContractResolver
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
            ValueProvider = new AttachmentsValueProvider(objRef),
            DeclaringType = type
        });

        return props;
    }
}

public class AttachmentsValueProvider(ObjectReferences objRef) : IValueProvider
{

    public object GetValue(object target)
    {
        if (!objRef.Attachements.TryGetValue(target, out var attachments))
        {
            attachments = new Dictionary<string, object>();
            objRef.Attachements[target] = attachments;
        }
        return objRef.Attachements[target];
    }

    public void SetValue(object target, object value)
    {
        objRef.Attachements[target] = (Dictionary<string, object>)value;
    }
}
