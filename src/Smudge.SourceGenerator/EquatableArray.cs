using System;

namespace Smudge.SourceGenerator;

internal readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>
    where T : IEquatable<T>
{
    private readonly T[]? _items;
    public EquatableArray(T[] items) => _items = items;
    public T[] Items => _items ?? Array.Empty<T>();

    public bool Equals(EquatableArray<T> other)
    {
        var a = Items;
        var b = other.Items;
        if (a.Length != b.Length) return false;
        for (var i = 0; i < a.Length; i++)
            if (!a[i].Equals(b[i])) return false;
        return true;
    }

    public override bool Equals(object? obj) => obj is EquatableArray<T> other && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;
            foreach (var item in Items)
                hash = hash * 31 + item.GetHashCode();
            return hash;
        }
    }
}