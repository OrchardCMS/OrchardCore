using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Orchard.DisplayManagement
{
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

        public static INamedEnumerable<object> From(object propertyObject)
        {
            var properties = propertyObject.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            var values = properties.Select(x => x.GetGetMethod().Invoke(propertyObject, null));
            return new NamedEnumerable<object>(values, properties.Select(x => x.Name));
        }

        class NamedEnumerable<T> : INamedEnumerable<T>
        {
            readonly List<T> _arguments;
            readonly List<string> _names;

            public NamedEnumerable(IEnumerable<T> arguments, IEnumerable<string> names)
            {
                if (arguments.Count() < names.Count())
                {
                    throw new ArgumentException("arguments.Count() < names.Count()");
                }

                _arguments = arguments.ToList();
                _names = names.ToList();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _arguments.GetEnumerator();
            }

            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                return _arguments.GetEnumerator();
            }

            IEnumerable<T> INamedEnumerable<T>.Positional
            {
                get { return _arguments.Take(_arguments.Count() - _names.Count()); }
            }

            IDictionary<string, T> INamedEnumerable<T>.Named
            {
                get { return new Named(_arguments, _names); }
            }

            class Named : IDictionary<string, T>
            {
                private readonly IEnumerable<T> _arguments;
                private readonly IEnumerable<string> _names;

                private ICollection<T> _argumentsCollection;
                private ICollection<string> _namesCollection;

                public Named(IEnumerable<T> arguments, IEnumerable<string> names)
                {
                    _arguments = arguments.Skip(arguments.Count() - names.Count());
                    _names = names;
                }

                IEnumerable<KeyValuePair<string, T>> MakeEnumerable()
                {
                    return _arguments.Zip(_names, (arg, name) => new KeyValuePair<string, T>(name, arg));
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
                    return MakeEnumerable().Contains(item);
                }

                void ICollection<KeyValuePair<string, T>>.CopyTo(KeyValuePair<string, T>[] array, int arrayIndex)
                {
                    throw new NotImplementedException();
                }

                bool ICollection<KeyValuePair<string, T>>.Remove(KeyValuePair<string, T> item)
                {
                    throw new NotImplementedException();
                }

                int ICollection<KeyValuePair<string, T>>.Count
                {
                    get { return _names.Count(); }
                }

                bool ICollection<KeyValuePair<string, T>>.IsReadOnly
                {
                    get { return true; }
                }

                bool IDictionary<string, T>.ContainsKey(string key)
                {
                    return _names.Contains(key);
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
                    var pair = MakeEnumerable().FirstOrDefault(kv => kv.Key == key);

                    // pair is a value type. in case of key-miss,
                    // will default to key=(string)null,value=(object)null

                    value = pair.Value;
                    return pair.Key != null;
                }

                //TBD
                T IDictionary<string, T>.this[string key]
                {
                    get
                    {
                        return MakeEnumerable()
                            .Where(kv => kv.Key == key)
                            .Select(kv => kv.Value)
                            .FirstOrDefault();
                    }
                    set { throw new NotImplementedException(); }
                }

                ICollection<string> IDictionary<string, T>.Keys
                {
                    get
                    {
                        return _namesCollection = _namesCollection ?? _names as ICollection<string> ?? _names.ToArray();
                    }
                }

                ICollection<T> IDictionary<string, T>.Values
                {
                    get { return _argumentsCollection = _argumentsCollection ?? _arguments as ICollection<T> ?? _arguments.ToArray(); }
                }
            }
        }

        public static INamedEnumerable<object> Empty = From(Enumerable.Empty<object>(), Enumerable.Empty<string>());
    }
}