using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
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

        public IList<IContentPartDisplayDriver> GetDisplayModeDrivers(string partName, string displayMode)
        {
            var services = new List<IContentPartDisplayDriver>();

            if (_contentDisplayOptions.ContentPartOptions.TryGetValue(partName, out var contentPartDisplayOption))
            {
                foreach (var displayDriverOption in contentPartDisplayOption.DisplayDrivers)
                {
                    if (displayDriverOption.DisplayMode.Invoke(displayMode))
                    {
                        services.Add((IContentPartDisplayDriver)_serviceProvider.GetRequiredService(displayDriverOption.DisplayDriverType));
                    }
                }
            }

            return services;
        }

        public IList<IContentPartDisplayDriver> GetEditorDrivers(string partName, string editor)
        {
            var services = new List<IContentPartDisplayDriver>();

            if (_contentDisplayOptions.ContentPartOptions.TryGetValue(partName, out var contentPartDisplayOption))
            {
                foreach (var editorDriverOption in contentPartDisplayOption.EditorDrivers)
                {
                    if (editorDriverOption.Editor.Invoke(editor))
                    {
                        services.Add((IContentPartDisplayDriver)_serviceProvider.GetRequiredService(editorDriverOption.DisplayDriverType));
                    }
                }
            }

            return services;
        }
    }
}
