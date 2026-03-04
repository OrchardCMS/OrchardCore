using System.Collections;

namespace OrchardCore.DisplayManagement.Shapes;

/// <summary>
/// An ordered collection optimized for lookups. All items are stored as array
/// segments in insertion order. <see cref="AddRange(string[])"/> stores the
/// array by reference (zero-copy). <see cref="Add"/> appends to an internal
/// list. When segments and the list coexist, the logical order is segments
/// first, then list items.
/// </summary>
public class AlternatesCollection : IEnumerable<string>
{
    public static readonly AlternatesCollection Empty = [];

    // Items added via Add().
    private List<string> _items;

    // First array segment (common case: only one).
    private string[] _segment;

    // Additional array segments, allocated only when more than one segment is added.
    private List<string[]> _additionalSegments;

    // Cached total count across all segments.
    private int _segmentsCount;

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
            if (_segment != null)
            {
                if (index < _segment.Length)
                {
                    return _segment[index];
                }

                index -= _segment.Length;

                if (_additionalSegments != null)
                {
                    for (var s = 0; s < _additionalSegments.Count; s++)
                    {
                        var segment = _additionalSegments[s];
                        if (index < segment.Length)
                        {
                            return segment[index];
                        }

                        index -= segment.Length;
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
                return _items[_items.Count - 1];
            }

            if (_additionalSegments != null && _additionalSegments.Count > 0)
            {
                var lastSegment = _additionalSegments[_additionalSegments.Count - 1];
                return lastSegment[lastSegment.Length - 1];
            }

            if (_segment != null)
            {
                return _segment[_segment.Length - 1];
            }

            return "";
        }
    }

    public void Add(string alternate)
    {
        ArgumentNullException.ThrowIfNull(alternate);

        EnsureMutable();

        if (!Contains(alternate))
        {
            _items ??= [];
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

        // Fast path: item is in _items (added via Add()).
        if (_items != null && _items.Remove(alternate))
        {
            return;
        }

        // Item must be in a segment (or not present at all).
        if (_segment != null)
        {
            var index = Array.IndexOf(_segment, alternate);
            if (index >= 0)
            {
                _segment = RemoveFromSegment(_segment, index);
                _segmentsCount--;

                if (_segment == null && _additionalSegments != null && _additionalSegments.Count > 0)
                {
                    // Promote the first additional segment to be the main segment to avoid fragmentation.
                    _segment = _additionalSegments[0];
                    _additionalSegments.RemoveAt(0);
                }

                return;
            }

            if (_additionalSegments != null)
            {
                for (var i = 0; i < _additionalSegments.Count; i++)
                {
                    var segment = _additionalSegments[i];
                    index = Array.IndexOf(segment, alternate);
                    if (index >= 0)
                    {
                        var newSegment = RemoveFromSegment(segment, index);
                        if (newSegment == null)
                        {
                            _additionalSegments.RemoveAt(i);
                        }
                        else
                        {
                            _additionalSegments[i] = newSegment;
                        }

                        _segmentsCount--;
                        return;
                    }
                }
            }
        }
    }

    public void Clear()
    {
        if (Count == 0)
        {
            return;
        }

        EnsureMutable();

        _items?.Clear();
        _segment = null;
        _additionalSegments?.Clear();
        _segmentsCount = 0;
    }

    public bool Contains(string alternate)
    {
        ArgumentNullException.ThrowIfNull(alternate);

        if (_items != null && _items.Contains(alternate))
        {
            return true;
        }

        if (_segment != null)
        {
            if (Array.IndexOf(_segment, alternate) >= 0)
            {
                return true;
            }

            if (_additionalSegments != null)
            {
                for (var i = 0; i < _additionalSegments.Count; i++)
                {
                    if (Array.IndexOf(_additionalSegments[i], alternate) >= 0)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    public int Count => _segmentsCount + (_items?.Count ?? 0);

    public void AddRange(AlternatesCollection alternates)
    {
        if (alternates._segment != null)
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

        EnsureMutable();

        // Flush any pending Add() items as a segment first so the array
        // segment is appended after them, preserving call order.
        FlushItems();

        AppendSegment(alternates);
    }

    private void AppendSegment(string[] segment)
    {
        if (_segment == null)
        {
            _segment = segment;
        }
        else
        {
            _additionalSegments ??= [];
            _additionalSegments.Add(segment);
        }

        _segmentsCount += segment.Length;
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

        AppendSegment([.. _items]);
        _items.Clear();
    }

    private static string[] RemoveFromSegment(string[] segment, int index)
    {
        if (segment.Length == 1)
        {
            return null;
        }

        var newSegment = new string[segment.Length - 1];
        Array.Copy(segment, 0, newSegment, 0, index);
        Array.Copy(segment, index + 1, newSegment, index, segment.Length - index - 1);
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
        if (_segment != null)
        {
            for (var i = 0; i < _segment.Length; i++)
            {
                yield return _segment[i];
            }

            if (_additionalSegments != null)
            {
                for (var s = 0; s < _additionalSegments.Count; s++)
                {
                    var segment = _additionalSegments[s];
                    for (var i = 0; i < segment.Length; i++)
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
