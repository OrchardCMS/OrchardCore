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
        private readonly ContentDisplayOptions _contentDisplayOptions;
        public ContentPartDisplayDriverResolver(
            IServiceProvider serviceProvider,
            IOptions<ContentDisplayOptions> contentDisplayOptions
            )
        {
            _serviceProvider = serviceProvider;
            _contentDisplayOptions = contentDisplayOptions.Value;
        }

        public IReadOnlyList<IContentPartDisplayDriver> GetDisplayDrivers(string partName)
        {
            if (_contentDisplayOptions.ContentPartOptions.TryGetValue(partName, out var contentPartDisplayOption))
            {
                var services = new List<IContentPartDisplayDriver>();
                foreach (var resolver in contentPartDisplayOption.DisplayDrivers)
                {
                    services.Add((IContentPartDisplayDriver)_serviceProvider.GetService(resolver));
                }

                return services;
            }
            return null;
        }
    }
}
