namespace DynObjectStore;

using System.Collections;
using System.Reflection;

public class TypeInspector
{
    // can check properties of a class structure
    private readonly HashSet<Type> _visited = new();
    private readonly Stack<Type> _currentPath = new();
    private readonly HashSet<Type> _recursiveTypes = new();
    private readonly HashSet<Type> _noParameterlessCtorTypes = new();

    public void Analyze(Type rootType)
    {
        _visited.Clear();
        _recursiveTypes.Clear();
        _noParameterlessCtorTypes.Clear();

        Traverse(rootType);
    }

    public IEnumerable<Type> RecursiveTypes => _recursiveTypes;
    public IEnumerable<Type> TypesWithoutParameterlessConstructor => _noParameterlessCtorTypes;

    private void Traverse(Type type)
    {
        if (type == null || type == typeof(string) || type.IsPrimitive || type.IsEnum)
            return;

        if (_visited.Contains(type))
            return;

        if (_currentPath.Contains(type))
        {
            _recursiveTypes.Add(type);
            return;
        }

        _visited.Add(type);
        _currentPath.Push(type);

        // Detect missing parameterless constructor
        bool hasParameterless = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Any(c => c.GetParameters().Length == 0);
        if (!hasParameterless)
            _noParameterlessCtorTypes.Add(type);

        // Traverse fields and properties
        foreach (var member in GetSerializableMembers(type))
        {
            var memberType = GetMemberType(member);

            if (typeof(IEnumerable).IsAssignableFrom(memberType) && memberType != typeof(string))
            {
                Type? elementType = TryGetEnumerableElementType(memberType);
                if (elementType != null)
                    Traverse(elementType);
            }
            else
            {
                Traverse(memberType);
            }
        }

        _currentPath.Pop();
    }

    private static IEnumerable<MemberInfo> GetSerializableMembers(Type type)
    {
        return type.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(m =>
                (m is FieldInfo f && !f.IsStatic && !f.IsDefined(typeof(NonSerializedAttribute))) ||
                (m is PropertyInfo p && p.CanRead && p.GetIndexParameters().Length == 0));
    }

    private static Type GetMemberType(MemberInfo member) =>
        member switch
        {
            FieldInfo f => f.FieldType,
            PropertyInfo p => p.PropertyType,
            _ => throw new NotSupportedException()
        };

    private static Type? TryGetEnumerableElementType(Type enumerableType)
    {
        if (enumerableType.IsArray)
            return enumerableType.GetElementType();

        if (enumerableType.IsGenericType)
            return enumerableType.GetGenericArguments().FirstOrDefault();

        return typeof(object); // fallback if unknown
    }
}
