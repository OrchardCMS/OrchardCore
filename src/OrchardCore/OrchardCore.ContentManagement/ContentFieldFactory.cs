using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;

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

        public ContentFieldFactory(
            IEnumerable<ContentField> contentFields,
            IOptions<ContentFieldOptions> contentFieldOptions
            )
        {
            _contentFieldActivators = new Dictionary<string, ITypeActivator<ContentField>>();

            // Check DI container for registered parts.
            // This code can be removed in a future release.
            foreach (var contentField in contentFields)
            {
                var activatorType = typeof(GenericTypeActivator<,>).MakeGenericType(contentField.GetType(), typeof(ContentField));
                var activator = (ITypeActivator<ContentField>)Activator.CreateInstance(activatorType);
                _contentFieldActivators.Add(contentField.GetType().Name, activator);
            }

            // Check ContentFieldOptions for configured parts.
            foreach (var fieldOption in contentFieldOptions.Value.FieldOptions)
            {
                var activatorType = typeof(GenericTypeActivator<,>).MakeGenericType(fieldOption.Type, typeof(ContentField));
                var activator = (ITypeActivator<ContentField>)Activator.CreateInstance(activatorType);
                _contentFieldActivators.Add(fieldOption.Type.Name, activator);
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