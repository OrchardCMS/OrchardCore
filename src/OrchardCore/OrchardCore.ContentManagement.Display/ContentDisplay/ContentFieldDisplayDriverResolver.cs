using System;
using System.Collections.Generic;
using System.Linq;
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

        public IReadOnlyList<IContentFieldDisplayDriver> GetDriversForDisplay(string fieldName, string displayMode)
        {
            // Standard display mode is always supplied as null.
            if (string.IsNullOrEmpty(displayMode))
            {
                displayMode = "Standard";
            };

            if (_contentDisplayOptions.ContentFieldOptions.TryGetValue(fieldName, out var contentFieldDisplayOption))
            {
                var services = new List<IContentFieldDisplayDriver>();
                var resolvers = contentFieldDisplayOption.DisplayDrivers.Where(x => x.DisplayModes.Contains("*") || x.DisplayModes.Contains(displayMode));
 
                foreach (var resolver in resolvers)
                {
                    services.Add((IContentFieldDisplayDriver)_serviceProvider.GetService(resolver.DisplayDriverType));
                }

                return services;
            }

            return null;
        }

        public IReadOnlyList<IContentFieldDisplayDriver> GetDriversForEdit(string fieldName, string editor)
        {
            // Standard editor is always supplied as null.
            if (string.IsNullOrEmpty(editor))
            {
                editor = "Standard";
            };

            if (_contentDisplayOptions.ContentFieldOptions.TryGetValue(fieldName, out var contentFieldDisplayOption))
            {
                var services = new List<IContentFieldDisplayDriver>();
                var resolvers = contentFieldDisplayOption.DisplayDrivers.Where(x => x.Editors.Contains("*") || x.Editors.Contains(editor));

                foreach (var resolver in resolvers)
                {
                    services.Add((IContentFieldDisplayDriver)_serviceProvider.GetService(resolver.DisplayDriverType));
                }

                return services;
            }

            return null;
        }
    }
}
