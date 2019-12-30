using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;

namespace OrchardCore.ContentManagement.Display.ContentDisplay
{
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

        public IReadOnlyList<IContentPartDisplayDriver> GetDriversForDisplay(string partName)
        {
            if (_contentDisplayOptions.ContentPartOptions.TryGetValue(partName, out var contentPartDisplayOption))
            {
                var services = new List<IContentPartDisplayDriver>();
                foreach (var resolver in contentPartDisplayOption.DisplayDrivers)
                {
                    services.Add((IContentPartDisplayDriver)_serviceProvider.GetService(resolver.DisplayDriverType));
                }

                return services;
            }

            return null;
        }

        public IReadOnlyList<IContentPartDisplayDriver> GetDriversForEdit(string partName, string editor)
        {
            if (_contentDisplayOptions.ContentPartOptions.TryGetValue(partName, out var contentPartDisplayOption))
            {
                var services = new List<IContentPartDisplayDriver>();
                var resolvers = contentPartDisplayOption.DisplayDrivers.Where(x => x.Editors.Contains("*") || x.Editors.Contains(editor));

                foreach (var resolver in contentPartDisplayOption.DisplayDrivers)
                {
                    services.Add((IContentPartDisplayDriver)_serviceProvider.GetService(resolver.DisplayDriverType));
                }

                return services;
            }

            return null;
        }
    }
}
