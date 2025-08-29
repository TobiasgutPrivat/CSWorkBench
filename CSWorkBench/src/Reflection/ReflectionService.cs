// singleton service for caching type reflections
class ReflectionService
{
    private readonly Dictionary<Type, TypeInfo> types = [];

    public TypeInfo getTypeInfo(object obj)
    {
        Type type = obj.GetType();
        if (types.ContainsKey(type))
            return types[type];
        else
            return types[type] = new TypeInfo(type);
    }
}