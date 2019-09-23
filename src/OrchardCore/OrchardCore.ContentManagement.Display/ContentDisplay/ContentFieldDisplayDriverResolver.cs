using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace OrchardCore.ContentManagement.Display.ContentDisplay
{
    public class ContentFieldDisplayDriverResolver : IContentFieldDisplayDriverResolver
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ContentDisplayOptions _contentDisplayOptions;
        public ContentFieldDisplayDriverResolver(
            IServiceProvider serviceProvider,
            IOptions<ContentDisplayOptions> contentDisplayOptions
            )
        {
            _serviceProvider = serviceProvider;
            _contentDisplayOptions = contentDisplayOptions.Value;
        }

        public IReadOnlyList<IContentFieldDisplayDriver> GetDisplayDrivers(string fieldName)
        {
            if (_contentDisplayOptions.ContentFieldOptions.TryGetValue(fieldName, out var contentFieldDisplayOption))
            {
                var services = new List<IContentFieldDisplayDriver>();
                foreach (var resolver in contentFieldDisplayOption.DisplayDrivers)
                {
                    services.Add((IContentFieldDisplayDriver)_serviceProvider.GetService(resolver));
                }

                return services;
            }
            return null;
        }
    }
}
