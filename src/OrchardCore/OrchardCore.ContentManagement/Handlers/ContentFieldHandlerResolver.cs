using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace OrchardCore.ContentManagement.Handlers
{
    public class ContentFieldHandlerResolver : IContentFieldHandlerResolver
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ContentOptions _contentOptions;

        public ContentFieldHandlerResolver(
            IServiceProvider serviceProvider,
            IOptions<ContentOptions> contentDisplayOptions
            )
        {
            _serviceProvider = serviceProvider;
            _contentOptions = contentDisplayOptions.Value;
        }

        public IList<IContentFieldHandler> GetHandlers(string fieldName)
        {
            var services = new List<IContentFieldHandler>();

            if (_contentOptions.ContentFieldOptionsLookup.TryGetValue(fieldName, out var contentFieldOption))
            {
                foreach (var handlerOption in contentFieldOption.Handlers)
                {
                    services.Add((IContentFieldHandler)_serviceProvider.GetRequiredService(handlerOption));
                }
            }

            return services;
        }
    }
}
