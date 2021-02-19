using System;
using System.Collections.Generic;

namespace OrchardCore.Data
{
    public class StoreCollectionOptions
    {
        private readonly HashSet<Type> _supportedTypes = new HashSet<Type>();
        private readonly Dictionary<Type, string> _collectionTypes = new Dictionary<Type, string>();

        private readonly List<string> _collections = new List<string>();
        public IReadOnlyList<string> Collections => _collections;

        internal void AddCollectionSupport(Type type)
            => _supportedTypes.Add(type);

        internal void WithCollection(Type type, string collection)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (!_supportedTypes.Contains(type))
            {
                throw new InvalidOperationException($"{type.Name} does not support collections.");
            }

            _collectionTypes[type] = collection;

            // The collection can be null. This will override any pre existing collection name.
            // It is not added to the collections set so that there is no attempt to initialize the default collection.
            if (!String.IsNullOrEmpty(collection))
            {
                _collections.Add(collection);
            }
        }

        /// <summary>
        /// Provides the registered collection name or <see langword="null"/> if no collection is used.
        /// All session operations must use this method to provide support for collections.
        /// </summary>
        public string For(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (_collectionTypes.TryGetValue(type, out var collection))
            {
                return collection;
            }

            return null;
        }

        /// <summary>
        /// Provides the registered collection name or <see langword="null"/> if no collection is used. 
        /// All session operations must use this method to provide support for collections.
        /// </summary>
        public string For<T>() where T : class
            => For(typeof(T));        
    }
}
