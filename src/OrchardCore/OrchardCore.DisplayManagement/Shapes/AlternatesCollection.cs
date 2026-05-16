using System.Collections;

namespace OrchardCore.DisplayManagement.Shapes;

/// <summary>
/// An ordered collection optimized for lookups.
/// </summary>
public sealed class AlternatesCollection : IEnumerable<string>
{
    public static readonly AlternatesCollection Empty = [];

    private readonly OrderedDictionary<string, string> _items = new(StringComparer.Ordinal);

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
        get => index < _items.Count ? _items.GetAt(index).Value : "";
    }

    public string Last
    {
        get => _items.Count > 0 ? _items.GetAt(_items.Count - 1).Value : "";
    }

    public void Add(string alternate)
    {
        ArgumentNullException.ThrowIfNull(alternate);

        EnsureMutable();

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

        return _items.ContainsKey(alternate);
    }

    public int Count => _items.Count;

    public void AddRange(AlternatesCollection alternates)
    {
        ArgumentNullException.ThrowIfNull(alternates);

        for (var i = 0; i < alternates._items.Count; i++)
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
        => _items.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => _items.Values.GetEnumerator();
}
