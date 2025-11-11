using System.Collections;
using System.Collections.Frozen;

namespace OrchardCore.DisplayManagement;

public static class Arguments
{
    public static INamedEnumerable<T> FromT<T>(IEnumerable<T> arguments, IEnumerable<string> names)
    {
        return new NamedEnumerable<T>(arguments, names);
    }

    public static INamedEnumerable<object> From(IEnumerable<object> arguments, IEnumerable<string> names)
    {
        return new NamedEnumerable<object>(arguments, names);
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
        return new NamedEnumerable<string>(dictionary.Values, dictionary.Keys);
    }

    public static INamedEnumerable<string> From(Dictionary<string, string> dictionary)
    {
        return new NamedEnumerable<string>(dictionary.Values, dictionary.Keys);
    }

    /// <summary>
    /// Creates an <see cref="INamedEnumerable{T}"/> from an object's properties.
    /// This method uses a compile-time generated accessor for types implementing <see cref="IArgumentsProvider"/>.
    /// For other types, it will use a slower fallback that caches reflection operations.
    /// </summary>
    /// <remarks>
    /// For optimal performance, types should implement <see cref="IArgumentsProvider"/> and use source generators
    /// or manual implementation to provide property access without reflection.
    /// </remarks>
    public static INamedEnumerable<object> From<T>(T propertyObject) where T : notnull
    {
        // Fast path for types that implement IArgumentsProvider
        if (propertyObject is IArgumentsProvider provider)
        {
            return provider.GetArguments();
        }

        // Fallback to cached reflection-based approach for anonymous types and other objects
        return ArgumentsReflectionHelper.FromReflection(propertyObject);
    }

    private sealed class NamedEnumerable<T> : INamedEnumerable<T>
    {
        private readonly T[] _arguments;
        private readonly string[] _names;
        private readonly ArraySegment<T> _positional;
        private IDictionary<string, T> _named;

        public NamedEnumerable(IEnumerable<T> arguments, IEnumerable<string> names)
        {
            _arguments = arguments as T[] ?? arguments.ToArray();
            _names = names as string[] ?? names.ToArray();

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

    public static readonly INamedEnumerable<object> Empty = From([], []);
}
