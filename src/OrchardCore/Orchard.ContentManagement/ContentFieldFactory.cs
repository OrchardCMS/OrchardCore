using System;
using System.Collections.Generic;

namespace OrchardCore.ContentManagement
{
    /// <summary>
    /// Implements <see cref="ITypeActivatorFactory{ContentField}"/> by resolving all registered <see cref="ContentField"/> types
    /// and memoizing a statically typed <see cref="ITypeActivator{ContentField}"/>.
    /// </summary>
    public class ContentFieldFactory : ITypeActivatorFactory<ContentField>
    {
        private ITypeActivator<ContentField> ContentFieldActivator = new GenericTypeActivator<ContentField, ContentField>();

        private readonly Dictionary<string, ITypeActivator<ContentField>> _contentFieldActivators;

        public ContentFieldFactory(IEnumerable<ContentField> contentFields)
        {
            _contentFieldActivators = new Dictionary<string, ITypeActivator<ContentField>>();

            foreach (var contentField in contentFields)
            {
                var activatorType = typeof(GenericTypeActivator<,>).MakeGenericType(contentField.GetType(), typeof(ContentField));
                var activator = (ITypeActivator<ContentField>)Activator.CreateInstance(activatorType);
                _contentFieldActivators.Add(contentField.GetType().Name, activator);
            }
        }

        /// <inheritdoc />
        public ITypeActivator<ContentField> GetTypeActivator(string fieldName)
        {
            if (_contentFieldActivators.TryGetValue(fieldName, out var activator))
            {
                return activator;
            }

            return ContentFieldActivator;
        }
    }
}