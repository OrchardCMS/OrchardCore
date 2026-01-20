using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace OrchardCore.ContentManagement.Handlers
{
    public class ContentPartHandlerResolver : IContentPartHandlerResolver
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ContentOptions _contentOptions;

        public ContentPartHandlerResolver(
            IServiceProvider serviceProvider,
            IOptions<ContentOptions> contentDisplayOptions
            )
        {
            _serviceProvider = serviceProvider;
            _contentOptions = contentDisplayOptions.Value;
        }

        public IList<IContentPartHandler> GetHandlers(string partName)
        {
            var services = new List<IContentPartHandler>();

            if (_contentOptions.ContentPartOptionsLookup.TryGetValue(partName, out var contentPartOption))
            {
                foreach (var handlerOption in contentPartOption.Handlers)
                {
                    services.Add((IContentPartHandler)_serviceProvider.GetRequiredService(handlerOption));
                }
            }

            return services;
        }
    }
}
