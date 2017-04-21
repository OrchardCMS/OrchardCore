using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Orchard.ContentManagement
{
    /// <summary>
    /// Implements <see cref="ITypeActivatorFactory{ContentField}"/> by resolving all registered <see cref="ContentField"/> types
    /// and memoizing a statically typed <see cref="ITypeActivator{ContentField}"/>.
    /// </summary>
    public class ContentFieldFactory : ITypeActivatorFactory<ContentField>
    {
        private ITypeActivator<ContentField> ContentFieldActivator = new GenericTypeActivator<ContentField, ContentField>();

        private readonly ConcurrentDictionary<string, ITypeActivator<ContentField>> _contentFieldActivators;

        public ContentFieldFactory(IEnumerable<ContentField> contentFields)
        {
            _contentFieldActivators = new ConcurrentDictionary<string, ITypeActivator<ContentField>>();

            foreach (var contentField in contentFields)
            {
                var activatorType = typeof(GenericTypeActivator<,>).MakeGenericType(contentField.GetType(), typeof(ContentField));
                var activator = (ITypeActivator<ContentField>)Activator.CreateInstance(activatorType);
                _contentFieldActivators.TryAdd(contentField.GetType().Name, activator);
            }
        }

        /// <inheritdoc />
        public ITypeActivator<ContentField> GetTypeActivator(string partName)
        {
            return _contentFieldActivators.GetOrAdd(partName, _ => ContentFieldActivator);
        }
    }
}