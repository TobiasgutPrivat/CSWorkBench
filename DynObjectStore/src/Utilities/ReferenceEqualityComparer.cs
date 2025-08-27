namespace DynObjectStore;

using System.Runtime.CompilerServices;

public sealed class ReferenceEqualityComparer : IEqualityComparer<object>
{
    public static readonly ReferenceEqualityComparer Instance = new();

    private ReferenceEqualityComparer() { }

    public new bool Equals(object? x, object? y) =>
        ReferenceEquals(x, y);

    public int GetHashCode(object obj) =>
        RuntimeHelpers.GetHashCode(obj);
}
