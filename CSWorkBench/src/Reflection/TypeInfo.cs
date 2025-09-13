using System.Reflection;

class TypeInfo
{
    public Type Type;
    public List<FieldInfo> fields = [];
    public List<PropertyInfo> properties = [];
    public List<MethodInfo> methods = [];

    public TypeInfo(Type type)
    {
        Type = type;
        fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance).ToList();
        properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();
        methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(m => !m.IsSpecialName && m.DeclaringType != typeof(object)).ToList();
    }
}