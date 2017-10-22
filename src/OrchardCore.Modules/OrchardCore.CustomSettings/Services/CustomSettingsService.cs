using System.Collections.Generic;
using System.Linq;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Metadata;

namespace OrchardCore.CustomSettings.Services
{
    public class CustomSettingsService
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public CustomSettingsService(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public IList<ContentTypeDefinition> GetSettingsTypes()
        {
            return _contentDefinitionManager
                .ListTypeDefinitions()
                .Where(x => x.Settings.ToObject<ContentTypeSettings>().Stereotype == "CustomSettings")
                .ToArray();
        }
    }
}
