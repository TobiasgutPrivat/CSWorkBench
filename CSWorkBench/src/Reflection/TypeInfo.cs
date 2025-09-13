using System.Reflection;

public class TypeInf
{
    public Type Type;
    public List<AttrInfo> fields = [];
    public List<MethodInfo> methods = [];

    public TypeInf(Type type)
    {
        Type = type;
        fields.AddRange(type.GetFields(BindingFlags.Public | BindingFlags.Instance).Select(f => new AttrInfo(f)));
        fields.AddRange(type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(p => new AttrInfo(p)));
        methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(m => !m.IsSpecialName && m.DeclaringType != typeof(object)).ToList();
    }
}