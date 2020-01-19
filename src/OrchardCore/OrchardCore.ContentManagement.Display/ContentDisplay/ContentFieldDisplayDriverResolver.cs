using System;
using System.Collections.Generic;
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

        public IList<IContentFieldDisplayDriver> GetDisplayModeDrivers(string fieldName, string displayMode)
        {
            var services = new List<IContentFieldDisplayDriver>();

            if (_contentDisplayOptions.ContentFieldOptions.TryGetValue(fieldName, out var contentFieldDisplayOption))
            {
                foreach (var displayModeDriverOption in contentFieldDisplayOption.DisplayModeDrivers)
                {
                    if (displayModeDriverOption.DisplayMode.Invoke(displayMode))
                    {
                        services.Add((IContentFieldDisplayDriver)_serviceProvider.GetRequiredService(displayModeDriverOption.DisplayDriverType));
                    }
                }
            }

            return services;
        }

        public IList<IContentFieldDisplayDriver> GetEditorDrivers(string fieldName, string editor)
        {
            var services = new List<IContentFieldDisplayDriver>();

            if (_contentDisplayOptions.ContentFieldOptions.TryGetValue(fieldName, out var contentFieldDisplayOption))
            {
                foreach (var editorDriverOption in contentFieldDisplayOption.EditorDrivers)
                {
                    if (editorDriverOption.Editor.Invoke(editor))
                    {
                        services.Add((IContentFieldDisplayDriver)_serviceProvider.GetRequiredService(editorDriverOption.DisplayDriverType));
                    }
                }
            }

            return services;
        }
    }
}
