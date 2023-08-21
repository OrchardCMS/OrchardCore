using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.Environment.Extensions.Features;

namespace OrchardCore.Environment.Shell.Builders
{
    /// <summary>
    /// A service collection that keeps track of the <see cref="IFeatureInfo"/> for each added service.
    /// </summary>
    public class FeatureAwareServiceCollection : IServiceCollection
    {
        private readonly IServiceCollection _innerServiceCollection;

        private readonly Dictionary<IFeatureInfo, ServiceCollection> _featureServiceCollections = new();
        private ServiceCollection _currentFeatureServiceCollection;

        public FeatureAwareServiceCollection(IServiceCollection innerServiceCollection)
        {
            _innerServiceCollection = innerServiceCollection;
        }

        /// <summary>
        /// A collection of services grouped by their feature information.
        /// </summary>
        public IDictionary<IFeatureInfo, ServiceCollection> FeatureCollections => _featureServiceCollections;

        /// <summary>
        /// Sets the current feature that services will be assigned when added to this collection.
        /// </summary>
        /// <param name="feature">The feature for services to be assigned.</param>
        public void SetCurrentFeature(IFeatureInfo feature)
        {
            if (!_featureServiceCollections.TryGetValue(feature, out _currentFeatureServiceCollection))
            {
                _featureServiceCollections.Add(feature, _currentFeatureServiceCollection = new ServiceCollection());
            }
        }

        public IEnumerator<ServiceDescriptor> GetEnumerator()
        {
            return _innerServiceCollection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        void ICollection<ServiceDescriptor>.Add(ServiceDescriptor item)
        {
            _innerServiceCollection.Add(item);
            _currentFeatureServiceCollection?.Add(item);
        }

        public void Clear()
        {
            _innerServiceCollection.Clear();
            _featureServiceCollections.Clear();
        }

        public bool Contains(ServiceDescriptor item)
        {
            return _innerServiceCollection.Contains(item);
        }

        public void CopyTo(ServiceDescriptor[] array, int arrayIndex)
        {
            _innerServiceCollection.CopyTo(array, arrayIndex);
        }

        public bool Remove(ServiceDescriptor item)
        {
            return _innerServiceCollection.Remove(item);
        }

        public int Count => _innerServiceCollection.Count;

        public bool IsReadOnly => _innerServiceCollection.IsReadOnly;

        public int IndexOf(ServiceDescriptor item)
        {
            return _innerServiceCollection.IndexOf(item);
        }

        public void Insert(int index, ServiceDescriptor item)
        {
            _innerServiceCollection.Insert(index, item);
            _currentFeatureServiceCollection?.Add(item);
        }

        public void RemoveAt(int index)
        {
            _innerServiceCollection.RemoveAt(index);
        }

        public ServiceDescriptor this[int index]
        {
            get => _innerServiceCollection[index];
            set => _innerServiceCollection[index] = value;
        }
    }
}
