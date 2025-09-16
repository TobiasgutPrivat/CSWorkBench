// singleton service for caching type reflections
using System.Reflection;

class ReflectionService
{
    private readonly Dictionary<Type, TypeInf> types = [];
    private readonly HashSet<Type> allTypes = [];

    public TypeInf getTypeInfo(object obj)
    {
        Type type = obj.GetType();
        if (types.ContainsKey(type))
            return types[type];
        else
            types[type] = new TypeInf(type);
        return types[type];
    }

    public List<Type> getAllTypes()
    {
        if (allTypes.Count > 0)
            return allTypes.ToList();
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (Assembly assembly in assemblies)
            allTypes.UnionWith(assembly.GetTypes());
        return allTypes.ToList();
    }
}