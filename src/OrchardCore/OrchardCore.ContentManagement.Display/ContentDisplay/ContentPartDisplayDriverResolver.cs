using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace OrchardCore.ContentManagement.Display.ContentDisplay
{
    public interface IContentPartDisplayDriverResolver
    {
        IReadOnlyList<IContentPartDisplayDriver> GetDisplayDrivers(string partName);
    }

    public class ContentPartDisplayDriverResolver : IContentPartDisplayDriverResolver
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ContentOptions _contentOptions;
        public ContentPartDisplayDriverResolver(
            IServiceProvider serviceProvider,
            IOptions<ContentOptions> contentOptions
            )
        {
            _serviceProvider = serviceProvider;
            _contentOptions = contentOptions.Value;
        }

        public IReadOnlyList<IContentPartDisplayDriver> GetDisplayDrivers(string partName)
        {
            if (_contentOptions.ContentPartOptionsLookup.TryGetValue(partName, out var displayTypeDriver))
            {
                var services = new List<IContentPartDisplayDriver>();
                var resolvers = displayTypeDriver.Resolvers[typeof(IContentPartDisplayDriver)];
                foreach (var resolver in resolvers)
                {
                    services.Add((IContentPartDisplayDriver)_serviceProvider.GetService(resolver));
                }

                return services;
            }
            return null;
        }
    }
}
