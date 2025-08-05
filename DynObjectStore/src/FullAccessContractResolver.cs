namespace DynObjectStore;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;

class FullAccessContractResolver : DefaultContractResolver
{
    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    {
        // Include public properties + fields + private fields
        var props = new List<JsonProperty>();

        // Traverse base classes to get private fields from base types
        Type? currenttype = type;
        while (currenttype != null)
        {
            var fields = currenttype
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(f => !f.IsDefined(typeof(JsonIgnoreAttribute), true));

            foreach (var field in fields)
            {
                var prop = new JsonProperty
                {
                    PropertyType = field.FieldType,
                    PropertyName = field.Name,
                    Readable = true,
                    Writable = true,
                    ValueProvider = new FieldValueProvider(field),
                    DeclaringType = currenttype,
                    IsReference = true,
                    ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                };

                props.Add(prop);
            }

            currenttype = currenttype.BaseType;
        }

        return props;
    }
}

class FieldValueProvider : IValueProvider
{
    private readonly FieldInfo field;

    public FieldValueProvider(FieldInfo field) => this.field = field;

    public object GetValue(object target) => field.GetValue(target)!;
    public void SetValue(object target, object? value) => field.SetValue(target, value);
}
