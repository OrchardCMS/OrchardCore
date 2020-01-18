using System;
using System.Collections.Generic;
using System.Linq;
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

        public IEnumerable<IContentPartDisplayDriver> GetDisplayDrivers(string partName)
        {
            if (_contentDisplayOptions.ContentPartOptions.TryGetValue(partName, out var contentPartDisplayOption))
            {
                var options = contentPartDisplayOption.DisplayDrivers.Where(x => x.Display.Invoke());

                return options.Select(r => (IContentPartDisplayDriver)_serviceProvider.GetRequiredService(r.DisplayDriverType));
            }

            return null;
        }

        public IEnumerable<IContentPartDisplayDriver> GetEditorDrivers(string partName, string editor)
        {
            editor = Normalize(editor);

            if (_contentDisplayOptions.ContentPartOptions.TryGetValue(partName, out var contentPartDisplayOption))
            {
                var options = contentPartDisplayOption.DisplayDrivers.Where(x => x.Editor.Invoke(editor));

                return options.Select(r => (IContentPartDisplayDriver)_serviceProvider.GetRequiredService(r.DisplayDriverType));
            }

            return null;
        }

        private static string Normalize(string editor)
        {
            // Standard editor is always supplied as null.
            if (string.IsNullOrEmpty(editor))
            {
                editor = "Standard";
            };

            return editor;
        }
    }
}
