using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace OrchardCore.DisplayManagement.Shapes
{
    /// <summary>
    /// An ordered collection optimized for lookups.
    /// </summary>
    public class AlternatesCollection : IEnumerable<string>
    {
        public static readonly AlternatesCollection Empty = new();

        private KeyedAlternateCollection _collection;

        public AlternatesCollection(params string[] alternates)
        {
            EnsureCollection();

            foreach (var alternate in alternates)
            {
                Add(alternate);
            }
        }

        public string this[int index] => _collection[index];

        public string Last => _collection.LastOrDefault() ?? "";

        public void Add(string alternate)
        {
            if (alternate == null)
            {
                throw new ArgumentNullException(nameof(alternate));
            }

            EnsureCollection();

            if (!_collection.Contains(alternate))
            {
                _collection.Add(alternate);
            }
        }

        public void Remove(string alternate)
        {
            if (alternate == null)
            {
                throw new ArgumentNullException(nameof(alternate));
            }

            if (_collection == null)
            {
                return;
            }

            _collection.Remove(alternate);
        }

        public void Clear()
        {
            if (_collection == null)
            {
                return;
            }

            _collection.Clear();
        }

        public bool Contains(string alternate)
        {
            if (alternate == null)
            {
                throw new ArgumentNullException(nameof(alternate));
            }

            if (_collection == null)
            {
                return false;
            }

            return _collection.Contains(alternate);
        }

        public int Count => _collection == null ? 0 : _collection.Count;

        public void AddRange(AlternatesCollection alternates)
        {
            AddRange(alternates._collection);
        }

        public void AddRange(IEnumerable<string> alternates)
        {
            if (alternates == null)
            {
                throw new ArgumentNullException(nameof(alternates));
            }

            if (alternates.Any())
            {
                EnsureCollection();

                foreach (var alternate in alternates)
                {
                    Add(alternate);
                }
            }
        }

        private void EnsureCollection()
        {
            _collection ??= new KeyedAlternateCollection();
        }

        public IEnumerator<string> GetEnumerator()
        {
            if (_collection == null)
            {
                return ((IEnumerable<string>)Array.Empty<string>()).GetEnumerator();
            }

            return _collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private class KeyedAlternateCollection : KeyedCollection<string, string>
        {
            protected override string GetKeyForItem(string item)
            {
                return item;
            }
        }
    }
}
