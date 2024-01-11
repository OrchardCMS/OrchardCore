using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace OrchardCore.ContentManagement
{
    /// <summary>
    /// Implements <see cref="ITypeActivatorFactory{ContentPart}"/> by resolving all registered <see cref="ContentPart"/> types
    /// and memoizing a statically typed <see cref="ITypeActivator{ContentPart}"/>.
    /// </summary>
    public class ContentPartFactory : ITypeActivatorFactory<ContentPart>
    {
        private readonly ITypeActivator<ContentPart> _contentPartActivator = new GenericTypeActivator<ContentPart, ContentPart>();

        private readonly Dictionary<string, ITypeActivator<ContentPart>> _contentPartActivators;

        public ContentPartFactory(IOptions<ContentOptions> contentOptions)
        {
            _contentPartActivators = new Dictionary<string, ITypeActivator<ContentPart>>();

            // Check content options for configured parts.
            foreach (var partOption in contentOptions.Value.ContentPartOptions)
            {
                var activatorType = typeof(GenericTypeActivator<,>).MakeGenericType(partOption.Type, typeof(ContentPart));
                var activator = (ITypeActivator<ContentPart>)Activator.CreateInstance(activatorType);
                _contentPartActivators.Add(partOption.Type.Name, activator);
            }
        }

        /// <inheritdoc />
        public ITypeActivator<ContentPart> GetTypeActivator(string partName)
        {
            if (_contentPartActivators.TryGetValue(partName, out var activator))
            {
                return activator;
            }

            return _contentPartActivator;
        }
    }
}
