using System.Collections;

namespace OrchardCore.DisplayManagement.Shapes;

/// <summary>
/// An ordered collection optimized for lookups.
/// </summary>
public sealed class AlternatesCollection : IEnumerable<string>
{
    public static readonly AlternatesCollection Empty = new();

    // Allocated on the first addition, as most shapes don't have any alternate or wrapper.
    private OrderedDictionary<string, string> _items;

    public AlternatesCollection(params string[] alternates)
    {
        if (alternates != null)
        {
            foreach (var alternate in alternates)
            {
                Add(alternate);
            }
        }
    }

    public string this[int index]
    {
        get => index < Count ? _items.GetAt(index).Value : "";
    }

    public string Last
    {
        get => Count > 0 ? _items.GetAt(_items.Count - 1).Value : "";
    }

    public void Add(string alternate)
    {
        ArgumentNullException.ThrowIfNull(alternate);

        EnsureMutable();

        _items ??= new OrderedDictionary<string, string>(StringComparer.Ordinal);
        _items.TryAdd(alternate, alternate);
    }

    public void Remove(string alternate)
    {
        ArgumentNullException.ThrowIfNull(alternate);

        if (Count == 0)
        {
            return;
        }

        EnsureMutable();

        _items.Remove(alternate);
    }

    public void Clear()
    {
        if (Count == 0)
        {
            return;
        }

        EnsureMutable();

        _items.Clear();
    }

    public bool Contains(string alternate)
    {
        ArgumentNullException.ThrowIfNull(alternate);

        return _items?.ContainsKey(alternate) == true;
    }

    public int Count => _items?.Count ?? 0;

    public void AddRange(AlternatesCollection alternates)
    {
        ArgumentNullException.ThrowIfNull(alternates);

        for (var i = 0; i < alternates.Count; i++)
        {
            Add(alternates._items.GetAt(i).Value);
        }
    }

    public void AddRange(IEnumerable<string> alternates)
    {
        ArgumentNullException.ThrowIfNull(alternates);

        EnsureMutable();

        foreach (var alternate in alternates)
        {
            Add(alternate);
        }
    }

    private void EnsureMutable()
    {
        if (this == Empty)
        {
            throw new NotSupportedException("AlternateCollection can't be changed.");
        }
    }

    public IEnumerator<string> GetEnumerator()
        => _items is null ? Enumerable.Empty<string>().GetEnumerator() : _items.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}
