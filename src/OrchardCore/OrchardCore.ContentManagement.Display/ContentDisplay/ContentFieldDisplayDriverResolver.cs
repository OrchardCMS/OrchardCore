using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
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

        public IEnumerable<IContentFieldDisplayDriver> GetDisplayModeDrivers(string fieldName, string displayMode)
        {
            displayMode = Normalize(displayMode);

            if (_contentDisplayOptions.ContentFieldOptions.TryGetValue(fieldName, out var contentFieldDisplayOption))
            {
                var options = contentFieldDisplayOption.DisplayDrivers.Where(x => x.DisplayMode.Invoke(displayMode));

                var services = options.Select(r => (IContentFieldDisplayDriver)_serviceProvider.GetRequiredService(r.DisplayDriverType));

                return services;
            }

            return null;
        }

        public IEnumerable<IContentFieldDisplayDriver> GetEditorDrivers(string fieldName, string editor)
        {
            editor = Normalize(editor);

            if (_contentDisplayOptions.ContentFieldOptions.TryGetValue(fieldName, out var contentFieldDisplayOption))
            {
                var options = contentFieldDisplayOption.DisplayDrivers.Where(x => x.Editor.Invoke(editor));

                var services = options.Select(r => (IContentFieldDisplayDriver)_serviceProvider.GetRequiredService(r.DisplayDriverType));

                return services;
            }

            return null;
        }

        private static string Normalize(string mode)
        {
            // Standard editor or display mode is always supplied as null.
            if (string.IsNullOrEmpty(mode))
            {
                mode = "Standard";
            };

            return mode;
        }
    }
}
