// singleton service for caching type reflections
class ReflectionService
{
    private readonly Dictionary<Type, TypeInf> types = [];

    public TypeInf getTypeInfo(object obj)
    {
        Type type = obj.GetType();
        if (types.ContainsKey(type))
            return types[type];
        else
            types[type] = new TypeInf(type);
            return types[type];
    }
}