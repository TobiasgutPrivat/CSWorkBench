namespace DynObjectStore;

using System.Collections.Concurrent;
using System.Reflection;

internal static class ReflectionCache
{
    private static readonly ConcurrentDictionary<Type, List<FieldInfo>> _fieldCache = new();

    /// <summary>
    /// Returns all serializable instance fields for the given type (cached).
    /// Includes private/protected fields in base classes.
    /// Skips [NonSerialized] and compiler-generated fields.
    /// </summary>
    public static List<FieldInfo> GetSerializableFields(Type type)
    {
        return _fieldCache.GetOrAdd(type, t =>
        {
            List<FieldInfo> fields = new List<FieldInfo>();
            Type? curr = t;

            while (curr != null && curr != typeof(object))
            {
                FieldInfo[] currfields = curr.GetFields(
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.DeclaredOnly);

                fields.AddRange(currfields);

                curr = curr.BaseType;
            }

            return fields;
        });
    }
}
