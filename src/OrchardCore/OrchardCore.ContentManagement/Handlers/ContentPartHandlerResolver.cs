using System;
using System.Collections.Generic;
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

        public IReadOnlyList<IContentPartHandler> GetHandlers(string partName)
        {
            if (_contentOptions.ContentPartOptionsLookup.TryGetValue(partName, out var contentPartOption))
            {
                var services = new List<IContentPartHandler>();
                foreach (var resolver in contentPartOption.Handlers)
                {
                    services.Add((IContentPartHandler)_serviceProvider.GetService(resolver));
                }

                return services;
            }
            return null;
        }
    }
}
