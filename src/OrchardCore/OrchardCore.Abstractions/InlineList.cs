#nullable enable

using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace OrchardCore;

/// <summary>
/// A highly efficient list implementation that stores up to 8 items inline without heap allocation.
/// Once the capacity exceeds 8 items, an array is allocated on the heap to store all items.
/// </summary>
/// <typeparam name="T">The type of elements in the list.</typeparam>
public struct InlineList<T> : IList<T>, IReadOnlyList<T>
{
    private const int OverflowAdditionalCapacity = 8;

    // Up to eight items are stored in an inline array. Once there are more items than will fit in the inline array,
    // an array is allocated to store all the items and the inline array is abandoned. Even if the size shrinks down
    // to below eight items, the array continues to be used.

    private InlineItems _items;
    private T[]? _overflowItems;
    private int _count;

    /// <summary>
    /// Initializes a new instance of the InlineList structure using the specified <paramref name="list" />.
    /// </summary>
    /// <param name="list">A span of items to initialize the list with.</param>
    public InlineList(params ReadOnlySpan<T> list) : this()
    {
        _count = list.Length;

        scoped Span<T> items = _count <= InlineItems.Length ?
            _items :
            _overflowItems = new T[_count + OverflowAdditionalCapacity];

        list.CopyTo(items);
    }

    /// <summary>
    /// Gets the number of items contained in the <see cref="InlineList{T}" />.
    /// </summary>
    public readonly int Count => _count;

    /// <summary>
    /// Gets a value indicating whether the <see cref="InlineList{T}" /> is read-only. This property will always return <see langword="false" />.
    /// </summary>
    public readonly bool IsReadOnly => false;

    /// <summary>
    /// Gets or sets the item at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element to get or set.</param>
    /// <returns>The item at the specified index.</returns>
    /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index" /> is not a valid index in the <see cref="InlineList{T}" />.</exception>
    public T this[int index]
    {
        readonly get
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint)index, (uint)_count, nameof(index));

            return _overflowItems is null ? _items[index] : _overflowItems[index];
        }
        set
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint)index, (uint)_count, nameof(index));

            if (_overflowItems is null)
            {
                _items[index] = value;
            }
            else
            {
                _overflowItems[index] = value;
            }
        }
    }

    /// <summary>
    /// Adds a item to the list.
    /// </summary>
    /// <param name="item">The item to add to the <see cref="InlineList{T}" />.</param>
    public void Add(T item)
    {
        int count = _count;

        if (_overflowItems is null && (uint)count < InlineItems.Length)
        {
            _items[count] = item;
            _count++;
        }
        else
        {
            AddToOverflow(item);
        }
    }

    /// <summary>
    /// Adds a item to the overflow list. Slow path outlined from Add to maximize the chance for the fast path to be inlined.
    /// </summary>
    /// <param name="item">The item to add to the overflow array.</param>
    private void AddToOverflow(T item)
    {
        Debug.Assert(_overflowItems is not null || _count == InlineItems.Length);

        if (_overflowItems is null)
        {
            _overflowItems = new T[InlineItems.Length + OverflowAdditionalCapacity];
            ((ReadOnlySpan<T>)_items).CopyTo(_overflowItems);
        }
        else if (_count == _overflowItems.Length)
        {
            Array.Resize(ref _overflowItems, _count + OverflowAdditionalCapacity);
        }

        _overflowItems[_count] = item;
        _count++;
    }

    /// <summary>
    /// Copies the contents of this  into a destination <paramref name="destination" /> span.
    /// </summary>
    /// <param name="destination">The destination <see cref="T:System.Span`1" /> object.</param>
    /// <exception cref="T:System.ArgumentException"> <paramref name="destination" /> The number of elements in the source <see cref="InlineList{T}" /> is greater than the number of elements that the destination span.</exception>
    public readonly void CopyTo(Span<T> destination)
    {
        if (destination.Length < _count)
        {
            throw new ArgumentException("The destination span is not long enough to copy all the items in the InlineList.", nameof(destination));
        }

        Items.CopyTo(destination);
    }

    /// <summary>
    /// Copies the entire <see cref="InlineList{T}" /> to a compatible one-dimensional array, starting at the specified index of the target array.
    /// </summary>
    /// <param name="array">The one-dimensional Array that is the destination of the elements copied from <see cref="InlineList{T}" />. The Array must have zero-based indexing.</param>
    /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
    /// <exception cref="T:System.ArgumentNullException"> <paramref name="array" /> is null.</exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException"> <paramref name="arrayIndex " /> is less than 0 or greater that or equal the <paramref name="array" /> length.</exception>
    public readonly void CopyTo(T[] array, int arrayIndex)
    {
        ArgumentNullException.ThrowIfNull(array);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint)arrayIndex, (uint)array.Length, nameof(arrayIndex));

        CopyTo(array.AsSpan(arrayIndex));
    }

    /// <summary>
    /// Inserts an element into the <see cref="InlineList{T}" /> at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index at which item should be inserted.</param>
    /// <param name="item">The item to insert.</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException"> <paramref name="index" /> index is less than 0 or <paramref name="index" /> is greater than <see cref="Count" />.</exception>
    public void Insert(int index, T item)
    {
        if (index == _count)
        {
            Add(item);
            return;
        }

        ArgumentOutOfRangeException.ThrowIfGreaterThan((uint)index, (uint)_count, nameof(index));

        if (_count == InlineItems.Length && _overflowItems is null)
        {
            _overflowItems = new T[InlineItems.Length + OverflowAdditionalCapacity];
            ((ReadOnlySpan<T>)_items).CopyTo(_overflowItems);
        }

        if (_overflowItems is not null)
        {
            if (_count == _overflowItems.Length)
            {
                Array.Resize(ref _overflowItems, _count + OverflowAdditionalCapacity);
            }

            _overflowItems.AsSpan(index, _count - index).CopyTo(_overflowItems.AsSpan(index + 1));
            _overflowItems[index] = item;
        }
        else
        {
            Span<T> items = _items;
            items.Slice(index, _count - index).CopyTo(items.Slice(index + 1));
            items[index] = item;
        }

        _count++;
    }

    /// <summary>
    /// Removes the element at the specified index of the <see cref="InlineList{T}" />.
    /// </summary>
    /// <param name="index">The zero-based index of the element to remove.</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException"> <paramref name="index" /> index is less than 0 or <paramref name="index" /> is greater than <see cref="Count" />.</exception>
    public void RemoveAt(int index)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint)index, (uint)_count, nameof(index));

        Span<T> items = _overflowItems is not null ? _overflowItems : _items;
        items.Slice(index + 1, _count - index - 1).CopyTo(items.Slice(index));
        _count--;
    }

    /// <summary>
    /// Removes all elements from the <see cref="InlineList{T}" />.
    /// </summary>
    public void Clear() =>
        _count = 0;

    /// <summary>
    /// Determines whether an item is in the <see cref="InlineList{T}" />.
    /// </summary>
    /// <param name="item">The item to locate in the <see cref="InlineList{T}" />.</param>
    /// <returns><see langword="true" /> if item is found in the <see cref="InlineList{T}" />; otherwise, <see langword="false" />.</returns>
    public readonly bool Contains(T item) =>
        IndexOf(item) >= 0;

    /// <summary>
    /// Removes the first occurrence of a specific object from the <see cref="InlineList{T}" />.
    /// </summary>
    /// <param name="item">The item to remove from the <see cref="InlineList{T}" />.</param>
    /// <returns><see langword="true" /> if item is successfully removed; otherwise, <see langword="false" />. This method also returns <see langword="false" /> if item was not found in the <see cref="InlineList{T}" />.</returns>
    public bool Remove(T item)
    {
        int index = IndexOf(item);
        if (index >= 0)
        {
            RemoveAt(index);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Returns an enumerator that iterates through the <see cref="InlineList{T}" />.
    /// </summary>
    /// <returns>Returns an enumerator that iterates through the <see cref="InlineList{T}" />.</returns>
    public readonly IEnumerator<T> GetEnumerator() => new Enumerator(in this);

    /// <summary>
    /// Returns an enumerator that iterates through the <see cref="InlineList{T}" />.
    /// </summary>
    /// <returns>An <see cref="IEnumerator"/> object that can be used to iterate through the collection.</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }


    /// <summary>
    /// Searches for the specified item and returns the zero-based index of the first occurrence within the entire <see cref="InlineList{T}" />.
    /// </summary>
    /// <param name="item">The item to locate in the <see cref="InlineList{T}" />.</param>
    /// <returns>The zero-based index of the first occurrence within the <see cref="InlineList{T}" />, or -1 if there is no such item.</returns>
    public readonly int IndexOf(T item)
    {
        ReadOnlySpan<T> items =
            _overflowItems is not null ? _overflowItems :
            _items;

        items = items.Slice(0, _count);

        for (int i = 0; i < items.Length; i++)
        {
            if (Equals(item, items[i]))
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Gets a <see cref="ReadOnlySpan{T}"/> view of the items currently stored in the <see cref="InlineList{T}"/>.
    /// </summary>
    /// <remarks>
    /// This property provides efficient access to the underlying storage without copying.
    /// If the list has overflowed to heap storage, the span references the heap array.
    /// Otherwise, it references the inline storage.
    /// </remarks>
    [UnscopedRef]
    internal readonly ReadOnlySpan<T> Items =>
        _overflowItems is not null ? _overflowItems.AsSpan(0, _count) :
        ((ReadOnlySpan<T>)_items).Slice(0, _count);

    /// <summary>
    /// An inline array structure that stores up to 8 items without heap allocation.
    /// </summary>
    [InlineArray(8)]
    private struct InlineItems
    {
        /// <summary>
        /// The maximum number of items that can be stored inline.
        /// </summary>
        public const int Length = 8;
        private T _first;
    }

    /// <summary>
    /// Enumerates the elements of an <see cref="InlineList{T}"/>.
    /// </summary>
    public struct Enumerator : IEnumerator<T>
    {
        private InlineList<T> _list;
        private int _index;

        /// <summary>
        /// Initializes a new instance of the <see cref="Enumerator"/> struct.
        /// </summary>
        /// <param name="list">The <see cref="InlineList{T}"/> to enumerate.</param>
        internal Enumerator(in InlineList<T> list)
        {
            _index = -1;
            _list = list;
        }

        /// <summary>
        /// Gets the element at the current position of the enumerator.
        /// </summary>
        public T Current => _list[_index];

        /// <summary>
        /// Gets the element at the current position of the enumerator.
        /// </summary>
        object? System.Collections.IEnumerator.Current => Current;

        /// <summary>
        /// Releases all resources used by the <see cref="Enumerator"/>.
        /// </summary>
        public void Dispose() { _index = _list.Count; }

        /// <summary>
        /// Advances the enumerator to the next element of the <see cref="InlineList{T}"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the enumerator was successfully advanced to the next element; <see langword="false"/> if the enumerator has passed the end of the collection.</returns>
        public bool MoveNext()
        {
            _index++;
            return _index < _list.Count;
        }

        /// <summary>
        /// Sets the enumerator to its initial position, which is before the first element in the collection.
        /// </summary>
        public void Reset() => _index = -1;
    }
}
