using System.Collections;

namespace OrchardCore.DisplayManagement.Shapes;

/// <summary>
/// An ordered collection of unique alternates.
/// </summary>
/// <remarks>
/// A shape has a few alternates at most, so they are stored in a list and compared linearly, which allocates
/// less than a hashed collection and is as fast for such small counts.
/// </remarks>
public sealed class AlternatesCollection : IEnumerable<string>
{
    public static readonly AlternatesCollection Empty = new();

    // Allocated on the first addition, as most shapes don't have any alternate or wrapper.
    private List<string> _items;

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
        get => index < Count ? _items[index] : "";
    }

    public string Last
    {
        get => Count > 0 ? _items[^1] : "";
    }

    public void Add(string alternate)
    {
        ArgumentNullException.ThrowIfNull(alternate);

        EnsureMutable();

        _items ??= new List<string>(4);

        if (!Contains(alternate))
        {
            _items.Add(alternate);
        }
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

        if (_items is null)
        {
            return false;
        }

        // Iterated by index to not allocate an enumerator, the collection is small.
        for (var i = 0; i < _items.Count; i++)
        {
            if (string.Equals(_items[i], alternate, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    public int Count => _items?.Count ?? 0;

    public void AddRange(AlternatesCollection alternates)
    {
        ArgumentNullException.ThrowIfNull(alternates);

        for (var i = 0; i < alternates.Count; i++)
        {
            Add(alternates._items[i]);
        }
    }

    public void AddRange(IEnumerable<string> alternates)
    {
        ArgumentNullException.ThrowIfNull(alternates);

        EnsureMutable();

        if (alternates is string[] array)
        {
            // The alternates factories provide arrays, iterated without an enumerator.
            for (var i = 0; i < array.Length; i++)
            {
                Add(array[i]);
            }

            return;
        }

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
        => _items is null ? Enumerable.Empty<string>().GetEnumerator() : _items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}
