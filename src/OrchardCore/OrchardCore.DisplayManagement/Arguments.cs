using System.Collections;
using System.Collections.Frozen;

namespace OrchardCore.DisplayManagement;

public static class Arguments
{
    public static INamedEnumerable<T> FromT<T>(IEnumerable<T> arguments, IEnumerable<string> names)
    {
        var argumentsArray = arguments as T[] ?? arguments.ToArray();
        var namesArray = names as string[] ?? names.ToArray();

        return argumentsArray.Length == 0 && namesArray.Length == 0
            ? Arguments<T>.Empty
            : new NamedEnumerable<T>(argumentsArray, namesArray);
    }

    public static INamedEnumerable<object> From(IEnumerable<object> arguments, IEnumerable<string> names)
    {
        var argumentsArray = arguments as object[] ?? arguments.ToArray();
        var namesArray = names as string[] ?? names.ToArray();

        return argumentsArray.Length == 0 && namesArray.Length == 0
            ? Empty
            : new NamedEnumerable<object>(argumentsArray, namesArray);
    }

    public static INamedEnumerable<object> From(IDictionary<string, object> dictionary)
    {
        return From(dictionary.Values, dictionary.Keys);
    }

    public static INamedEnumerable<object> From(Dictionary<string, object> dictionary)
    {
        return From(dictionary.Values, dictionary.Keys);
    }

    public static INamedEnumerable<string> From(IDictionary<string, string> dictionary)
    {
        if (dictionary.Count == 0)
        {
            return Arguments<string>.Empty;
        }

        return new NamedEnumerable<string>(dictionary.Values.ToArray(), dictionary.Keys.ToArray());
    }

    public static INamedEnumerable<string> From(Dictionary<string, string> dictionary)
    {
        if (dictionary.Count == 0)
        {
            return Arguments<string>.Empty;
        }

        return new NamedEnumerable<string>(dictionary.Values.ToArray(), dictionary.Keys.ToArray());
    }

    /// <summary>
    /// Creates an <see cref="INamedEnumerable{T}"/> from an object's properties.
    /// For types marked with <see cref="GenerateArgumentsAttribute"/>, this uses compile-time generated property access.
    /// For other types, it will use a slower fallback that caches reflection operations.
    /// </summary>
    public static INamedEnumerable<object> From<T>(T propertyObject) where T : notnull
    {
        // Fast path for source-generated types that directly implement INamedEnumerable<object>
        if (propertyObject is INamedEnumerable<object> namedEnumerable)
        {
            return namedEnumerable;
        }

        // Fallback to cached reflection-based approach for anonymous types and other objects
        return ArgumentsReflectionHelper.FromReflection(propertyObject);
    }

    internal sealed class NamedEnumerable<T> : INamedEnumerable<T>
    {
        private readonly T[] _arguments;
        private readonly string[] _names;
        private readonly ArraySegment<T> _positional;
        private IDictionary<string, T> _named;

        public NamedEnumerable(T[] arguments, string[] names)
        {
            _arguments = arguments;
            _names = names;

            ArgumentOutOfRangeException.ThrowIfLessThan(_arguments.Length, _names.Length);

            var positionalCount = _arguments.Length - _names.Length;
            if (positionalCount > 0)
            {
                _positional = new ArraySegment<T>(_arguments, 0, positionalCount);
            }
            else
            {
                _positional = ArraySegment<T>.Empty;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _arguments.GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return ((IEnumerable<T>)_arguments).GetEnumerator();
        }

        IList<T> INamedEnumerable<T>.Positional => _positional;

        IDictionary<string, T> INamedEnumerable<T>.Named => _named ??= new Named(_arguments, _names);

        public int Count => _arguments.Length;

        private sealed class Named : IDictionary<string, T>
        {
            private readonly ArraySegment<T> _arguments;
            private readonly string[] _names;
            private FrozenDictionary<string, int> _nameToIndex;
            private IEnumerable<KeyValuePair<string, T>> _enumerable;

            public Named(T[] arguments, string[] names)
            {
                var positionalCount = arguments.Length - names.Length;

                if (positionalCount > 0)
                {
                    _arguments = new ArraySegment<T>(arguments, positionalCount, names.Length);
                }
                else
                {
                    _arguments = arguments;
                }

                _names = names;
            }

            private void EnsureNameToIndex()
            {
                if (_nameToIndex != null)
                {
                    return;
                }

                var indexMap = new Dictionary<string, int>(_names.Length);
                for (var i = 0; i < _names.Length; i++)
                {
                    indexMap[_names[i]] = i;
                }
                _nameToIndex = indexMap.ToFrozenDictionary();
            }

            private IEnumerable<KeyValuePair<string, T>> MakeEnumerable()
            {
                if (_enumerable != null)
                {
                    return _enumerable;
                }

                var pairs = new KeyValuePair<string, T>[_names.Length];
                for (var i = 0; i < _names.Length; i++)
                {
                    pairs[i] = new KeyValuePair<string, T>(_names[i], _arguments[i]);
                }
                return _enumerable = pairs;
            }

            IEnumerator<KeyValuePair<string, T>> IEnumerable<KeyValuePair<string, T>>.GetEnumerator()
            {
                return MakeEnumerable().GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return MakeEnumerable().GetEnumerator();
            }

            void ICollection<KeyValuePair<string, T>>.Add(KeyValuePair<string, T> item)
            {
                throw new NotImplementedException();
            }

            void ICollection<KeyValuePair<string, T>>.Clear()
            {
                throw new NotImplementedException();
            }

            bool ICollection<KeyValuePair<string, T>>.Contains(KeyValuePair<string, T> item)
            {
                EnsureNameToIndex();

                return _nameToIndex.TryGetValue(item.Key, out var index) &&
                       EqualityComparer<T>.Default.Equals(_arguments[index], item.Value);
            }

            void ICollection<KeyValuePair<string, T>>.CopyTo(KeyValuePair<string, T>[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            bool ICollection<KeyValuePair<string, T>>.Remove(KeyValuePair<string, T> item)
            {
                throw new NotImplementedException();
            }

            int ICollection<KeyValuePair<string, T>>.Count => _names.Length;

            bool ICollection<KeyValuePair<string, T>>.IsReadOnly => true;

            bool IDictionary<string, T>.ContainsKey(string key)
            {
                EnsureNameToIndex();
                return _nameToIndex.ContainsKey(key);
            }

            void IDictionary<string, T>.Add(string key, T value)
            {
                throw new NotImplementedException();
            }

            bool IDictionary<string, T>.Remove(string key)
            {
                throw new NotImplementedException();
            }

            bool IDictionary<string, T>.TryGetValue(string key, out T value)
            {
                EnsureNameToIndex();

                if (_nameToIndex.TryGetValue(key, out var index))
                {
                    value = _arguments[index];
                    return true;
                }

                value = default;
                return false;
            }

            T IDictionary<string, T>.this[string key]
            {
                get
                {
                    EnsureNameToIndex();

                    return _nameToIndex.TryGetValue(key, out var index) ? _arguments[index] : default;
                }
                set { throw new NotImplementedException(); }
            }

            ICollection<string> IDictionary<string, T>.Keys => _names;

            ICollection<T> IDictionary<string, T>.Values => _arguments;
        }
    }

    public static readonly INamedEnumerable<object> Empty = new NamedEnumerable<object>([], []);
}

public static class Arguments<T>
{
    public static readonly INamedEnumerable<T> Empty = new Arguments.NamedEnumerable<T>([], []);
}

/// <summary>
/// Helper class for implementing property-based INamedEnumerable.
/// Used by source-generated code to avoid allocating arrays.
/// </summary>
public abstract class PropertyBasedNamedEnumerable : INamedEnumerable<object>
{
    private IDictionary<string, object> _named;

    protected abstract int PropertyCount { get; }
    protected abstract IReadOnlyList<string> PropertyNames { get; }
    protected abstract object GetPropertyValue(int index);

    IList<object> INamedEnumerable<object>.Positional => Array.Empty<object>();

    IDictionary<string, object> INamedEnumerable<object>.Named => _named ??= new PropertyDictionary(this);

    int IReadOnlyCollection<object>.Count => PropertyCount;

    IEnumerator<object> IEnumerable<object>.GetEnumerator()
    {
        for (var i = 0; i < PropertyCount; i++)
        {
            yield return GetPropertyValue(i);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<object>)this).GetEnumerator();

    private sealed class PropertyDictionary : IDictionary<string, object>
    {
        private readonly PropertyBasedNamedEnumerable _parent;
        private FrozenDictionary<string, int> _nameToIndex;
        private KeyValuePair<string, object>[] _pairs;

        public PropertyDictionary(PropertyBasedNamedEnumerable parent)
        {
            _parent = parent;
        }

        private void EnsureNameToIndex()
        {
            if (_nameToIndex != null)
            {
                return;
            }

            var names = _parent.PropertyNames;
            var indexMap = new Dictionary<string, int>(names.Count);
            for (var i = 0; i < names.Count; i++)
            {
                indexMap[names[i]] = i;
            }
            _nameToIndex = indexMap.ToFrozenDictionary();
        }

        private KeyValuePair<string, object>[] GetPairs()
        {
            if (_pairs != null)
            {
                return _pairs;
            }

            var names = _parent.PropertyNames;
            var pairs = new KeyValuePair<string, object>[names.Count];
            for (var i = 0; i < names.Count; i++)
            {
                pairs[i] = new KeyValuePair<string, object>(names[i], _parent.GetPropertyValue(i));
            }
            return _pairs = pairs;
        }

        public bool TryGetValue(string key, out object value)
        {
            EnsureNameToIndex();

            if (_nameToIndex.TryGetValue(key, out var index))
            {
                value = _parent.GetPropertyValue(index);
                return true;
            }

            value = default;
            return false;
        }

        public object this[string key]
        {
            get
            {
                EnsureNameToIndex();
                return _nameToIndex.TryGetValue(key, out var index) ? _parent.GetPropertyValue(index) : default;
            }
            set => throw new NotImplementedException();
        }

        public bool ContainsKey(string key)
        {
            EnsureNameToIndex();
            return _nameToIndex.ContainsKey(key);
        }

        public ICollection<string> Keys => (ICollection<string>)_parent.PropertyNames;

        public ICollection<object> Values => GetPairs().Select(p => p.Value).ToArray();

        public int Count => _parent.PropertyCount;

        public bool IsReadOnly => true;

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => ((IEnumerable<KeyValuePair<string, object>>)GetPairs()).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool Contains(KeyValuePair<string, object> item)
        {
            return TryGetValue(item.Key, out var value) && EqualityComparer<object>.Default.Equals(value, item.Value);
        }

        public void Add(string key, object value) => throw new NotImplementedException();
        public void Add(KeyValuePair<string, object> item) => throw new NotImplementedException();
        public bool Remove(string key) => throw new NotImplementedException();
        public bool Remove(KeyValuePair<string, object> item) => throw new NotImplementedException();
        public void Clear() => throw new NotImplementedException();
        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) => throw new NotImplementedException();
    }
}
