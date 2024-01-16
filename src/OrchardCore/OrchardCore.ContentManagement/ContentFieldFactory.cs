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
        private readonly ITypeActivator<ContentField> _contentFieldActivator = new GenericTypeActivator<ContentField, ContentField>();

        private readonly Dictionary<string, ITypeActivator<ContentField>> _contentFieldActivators;

        public ContentFieldFactory(IOptions<ContentOptions> contentOptions)
        {
            _contentFieldActivators = new Dictionary<string, ITypeActivator<ContentField>>();

            // Check content options for configured fields.
            foreach (var fieldOption in contentOptions.Value.ContentFieldOptions)
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

            return _contentFieldActivator;
        }
    }
}
