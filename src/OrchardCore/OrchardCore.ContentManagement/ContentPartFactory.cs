using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace OrchardCore.ContentManagement
{
    /// <summary>
    /// Implements <see cref="ITypeActivatorFactory{ContentPart}"/> by resolving all registered <see cref="ContentPart"/> types
    /// and memoizing a statically typed <see cref="ITypeActivator{ContentPart}"/>.
    /// </summary>
    public class ContentPartFactory : ITypeActivatorFactory<ContentPart>
    {
        private ITypeActivator<ContentPart> ContentPartActivator = new GenericTypeActivator<ContentPart, ContentPart>();

        private readonly Dictionary<string, ITypeActivator<ContentPart>> _contentPartActivators;

        public ContentPartFactory(
            IEnumerable<ContentPart> contentParts,
            IOptions<ContentPartOptions> contentPartOptions
            )
        {
            _contentPartActivators = new Dictionary<string, ITypeActivator<ContentPart>>();

            // Check DI container for registered parts
            foreach (var contentPart in contentParts)
            {
                var activatorType =  typeof(GenericTypeActivator<,>).MakeGenericType(contentPart.GetType(), typeof(ContentPart));
                var activator = (ITypeActivator<ContentPart>)Activator.CreateInstance(activatorType);
                _contentPartActivators.Add(contentPart.GetType().Name, activator);
            }

            // Check ContentPartOptions for configured parts.
            // TODO possibly check here, that it's not already registered.
            foreach (var partOption in contentPartOptions.Value.PartOptions)
            {
                var activatorType =  typeof(GenericTypeActivator<,>).MakeGenericType(partOption.Type, typeof(ContentPart));
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

            return ContentPartActivator;
        }
    }
}