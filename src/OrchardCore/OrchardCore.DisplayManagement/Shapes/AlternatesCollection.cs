using System.Collections;

namespace OrchardCore.DisplayManagement.Shapes;

/// <summary>
/// An ordered collection optimized for lookups. All items are stored as array
/// segments in insertion order. <see cref="AddRange(string[])"/> stores the
/// array by reference (zero-copy). <see cref="Add"/> appends to an internal
/// list. When segments and the list coexist, the logical order is segments
/// first, then list items.
/// </summary>
public sealed class AlternatesCollection : IEnumerable<string>
{
    public static readonly AlternatesCollection Empty = [];

    private readonly Dictionary<string, string> _dictionary = new(StringComparer.Ordinal);
    private List<string> _items;

    // First array segment (common case: only one).
    private ArraySegment<string> _segment;

    // Additional array segments, allocated only when more than one segment is added.
    private List<ArraySegment<string>> _additionalSegments;

    private int _count;

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
        get
        {
            if (_segment.Array != null)
            {
                if (index < _segment.Count)
                {
                    return _segment[index];
                }

                index -= _segment.Count;

                if (_additionalSegments != null)
                {
                    for (var s = 0; s < _additionalSegments.Count; s++)
                    {
                        var segment = _additionalSegments[s];
                        if (index < segment.Count)
                        {
                            return segment[index];
                        }

                        index -= segment.Count;
                    }
                }
            }

            if (_items != null && index < _items.Count)
            {
                return _items[index];
            }

            return "";
        }
    }

    public string Last
    {
        get
        {
            if (_items != null && _items.Count > 0)
            {
                return _items[^1];
            }

            if (_additionalSegments != null && _additionalSegments.Count > 0)
            {
                var lastSegment = _additionalSegments[^1];
                return lastSegment[lastSegment.Count - 1];
            }

            if (_segment.Array != null)
            {
                return _segment[_segment.Count - 1];
            }

            return "";
        }
    }

    public void Add(string alternate)
    {
        ArgumentNullException.ThrowIfNull(alternate);

        EnsureMutable();

        if (_dictionary.TryAdd(alternate, alternate))
        {
            _items ??= [];
            _items.Add(alternate);
            _count++;
        }
    }

    public void Remove(string alternate)
    {
        ArgumentNullException.ThrowIfNull(alternate);

        if (_count == 0)
        {
            return;
        }

        EnsureMutable();

        if (!_dictionary.Remove(alternate))
        {
            return;
        }

        _count--;

        // Fast path: item is in _items (added via Add()).
        if (_items != null && _items.Remove(alternate))
        {
            return;
        }

        // Item must be in a segment.
        if (_segment.Array != null)
        {
            var index = Array.IndexOf(_segment.Array, alternate, _segment.Offset, _segment.Count);
            if (index >= 0)
            {
                var localIndex = index - _segment.Offset;

                if (_segment.Count == 1)
                {
                    _segment = default;

                    if (_additionalSegments != null && _additionalSegments.Count > 0)
                    {
                        // Promote the first additional segment to be the main segment.
                        _segment = _additionalSegments[0];
                        _additionalSegments.RemoveAt(0);
                    }
                }
                else if (localIndex == 0)
                {
                    // Remove from beginning: adjust offset
                    _segment = new ArraySegment<string>(_segment.Array, _segment.Offset + 1, _segment.Count - 1);
                }
                else if (localIndex == _segment.Count - 1)
                {
                    // Remove from end: adjust count
                    _segment = new ArraySegment<string>(_segment.Array, _segment.Offset, _segment.Count - 1);
                }
                else
                {
                    // Remove from middle: need to copy
                    var newSegment = RemoveFromSegment(_segment, localIndex);
                    _segment = new ArraySegment<string>(newSegment);
                }

                return;
            }

            if (_additionalSegments != null)
            {
                for (var i = 0; i < _additionalSegments.Count; i++)
                {
                    var segment = _additionalSegments[i];
                    index = Array.IndexOf(segment.Array, alternate, segment.Offset, segment.Count);
                    if (index >= 0)
                    {
                        var localIndex = index - segment.Offset;

                        if (segment.Count == 1)
                        {
                            _additionalSegments.RemoveAt(i);
                        }
                        else if (localIndex == 0)
                        {
                            // Remove from beginning: adjust offset
                            _additionalSegments[i] = new ArraySegment<string>(segment.Array, segment.Offset + 1, segment.Count - 1);
                        }
                        else if (localIndex == segment.Count - 1)
                        {
                            // Remove from end: adjust count
                            _additionalSegments[i] = new ArraySegment<string>(segment.Array, segment.Offset, segment.Count - 1);
                        }
                        else
                        {
                            // Remove from middle: need to copy
                            var newSegment = RemoveFromSegment(segment, localIndex);
                            _additionalSegments[i] = new ArraySegment<string>(newSegment);
                        }

                        return;
                    }
                }
            }
        }
    }

    public void Clear()
    {
        if (_count == 0)
        {
            return;
        }

        EnsureMutable();

        _dictionary.Clear();
        _items?.Clear();
        _segment = default;
        _additionalSegments?.Clear();
        _count = 0;
    }

    public bool Contains(string alternate)
    {
        ArgumentNullException.ThrowIfNull(alternate);

        return _dictionary.ContainsKey(alternate);
    }

    public int Count => _count;

    public void AddRange(AlternatesCollection alternates)
    {
        if (alternates._segment.Array != null)
        {
            AddRange(alternates._segment);

            if (alternates._additionalSegments != null)
            {
                for (var i = 0; i < alternates._additionalSegments.Count; i++)
                {
                    AddRange(alternates._additionalSegments[i]);
                }
            }
        }

        if (alternates._items != null)
        {
            for (var i = 0; i < alternates._items.Count; i++)
            {
                Add(alternates._items[i]);
            }
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

    /// <summary>
    /// Adds alternates from an array by reference. The array is not copied;
    /// its elements are accessed on demand. This is the fast path for cached alternates.
    /// </summary>
    /// <param name="alternates">The array of alternates to add.</param>
    public void AddRange(string[] alternates)
    {
        ArgumentNullException.ThrowIfNull(alternates);

        if (alternates.Length == 0)
        {
            return;
        }

        AddRange(new ArraySegment<string>(alternates));
    }

    private void AddRange(ArraySegment<string> segment)
    {
        if (segment.Count == 0)
        {
            return;
        }

        EnsureMutable();

        // Find the first non-duplicate element
        var start = segment.Offset;
        var end = segment.Offset + segment.Count;

        while (start < end && _dictionary.ContainsKey(segment.Array[start]))
        {
            start++;
        }

        // All elements are duplicates
        if (start == end)
        {
            return;
        }

        // Find the last non-duplicate element
        while (end > start && _dictionary.ContainsKey(segment.Array[end - 1]))
        {
            end--;
        }

        // Check if there are duplicates in the middle
        var hasMiddleDuplicates = false;
        for (var i = start; i < end; i++)
        {
            if (_dictionary.ContainsKey(segment.Array[i]))
            {
                hasMiddleDuplicates = true;
                break;
            }
        }

        if (hasMiddleDuplicates)
        {
            // Duplicates scattered throughout: fall back to individual additions
            for (var i = start; i < end; i++)
            {
                if (_dictionary.TryAdd(segment.Array[i], segment.Array[i]))
                {
                    _items ??= [];
                    _items.Add(segment.Array[i]);
                    _count++;
                }
            }
            return;
        }

        // No duplicates in the range [start, end) - use zero-copy segment
        var newSegment = new ArraySegment<string>(segment.Array, start, end - start);

        // Flush any pending Add() items as a segment first so the array
        // segment is appended after them, preserving call order.
        FlushItems();

        AppendSegment(newSegment);

        for (var i = start; i < end; i++)
        {
            _dictionary.Add(segment.Array[i], segment.Array[i]);
        }

        _count += newSegment.Count;
    }

    private void AppendSegment(ArraySegment<string> segment)
    {
        if (_segment.Array == null)
        {
            _segment = segment;
        }
        else
        {
            _additionalSegments ??= [];
            _additionalSegments.Add(segment);
        }
    }

    /// <summary>
    /// Converts pending <see cref="_items"/> into a segment so that they
    /// participate in segment enumeration in the correct position.
    /// </summary>
    private void FlushItems()
    {
        if (_items == null || _items.Count == 0)
        {
            return;
        }

        var itemsArray = _items.ToArray();
        AppendSegment(new ArraySegment<string>(itemsArray));
        _items.Clear();
    }

    private static string[] RemoveFromSegment(ArraySegment<string> segment, int localIndex)
    {
        // Create a new array without the element at localIndex
        var newSegment = new string[segment.Count - 1];

        if (localIndex > 0)
        {
            Array.Copy(segment.Array, segment.Offset, newSegment, 0, localIndex);
        }

        if (localIndex < segment.Count - 1)
        {
            Array.Copy(segment.Array, segment.Offset + localIndex + 1, newSegment, localIndex, segment.Count - localIndex - 1);
        }

        return newSegment;
    }

    private void EnsureMutable()
    {
        if (this == Empty)
        {
            throw new NotSupportedException("AlternateCollection can't be changed.");
        }
    }

    public IEnumerator<string> GetEnumerator()
    {
        return Enumerate().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private IEnumerable<string> Enumerate()
    {
        if (_segment.Array != null)
        {
            for (var i = 0; i < _segment.Count; i++)
            {
                yield return _segment[i];
            }

            if (_additionalSegments != null)
            {
                for (var s = 0; s < _additionalSegments.Count; s++)
                {
                    var segment = _additionalSegments[s];
                    for (var i = 0; i < segment.Count; i++)
                    {
                        yield return segment[i];
                    }
                }
            }
        }

        if (_items != null)
        {
            for (var i = 0; i < _items.Count; i++)
            {
                yield return _items[i];
            }
        }
    }
}
