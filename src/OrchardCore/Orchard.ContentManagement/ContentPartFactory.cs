using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Orchard.ContentManagement
{
    /// <summary>
    /// Implements <see cref="IContentPartFactory"/> by resolving all registered <see cref="ContentPart"/> types
    /// and memoizing a statically typed <see cref="IContentElementActivator"/>.
    /// </summary>
    public class ContentPartFactory : ITypeActivatorFactory<ContentPart>
    {
        private ITypeActivator<ContentPart> ContentPartActivator = new GenericTypeActivator<ContentPart, ContentPart>();

        private readonly ConcurrentDictionary<string, ITypeActivator<ContentPart>> _contentPartActivators;

        public ContentPartFactory(IEnumerable<ContentPart> contentParts)
        {
            _contentPartActivators = new ConcurrentDictionary<string, ITypeActivator<ContentPart>>();

            foreach (var contentPart in contentParts)
            {
                var activatorType =  typeof(GenericTypeActivator<,>).MakeGenericType(contentPart.GetType(), typeof(ContentPart));
                var activator = (ITypeActivator<ContentPart>)Activator.CreateInstance(activatorType);
                _contentPartActivators.TryAdd(contentPart.GetType().Name, activator);
            }
        }

        /// <inheritdoc />
        public ITypeActivator<ContentPart> GetTypeActivator(string partName)
        {
            return _contentPartActivators.GetOrAdd(partName, _ => ContentPartActivator);
        }
    }

    internal class GenericTypeActivator<T, TInstance> : ITypeActivator<TInstance> where T : TInstance, new()
    {
        /// <inheritdoc />
        public Type Type => typeof(T);

        /// <inheritdoc />
        public TInstance CreateInstance()
        {
            return new T();
        }
    }
}