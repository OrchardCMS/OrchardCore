using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement.Metadata.Models;
using Orchard.ContentManagement.Metadata.Settings;
using Orchard.ContentManagement.MetaData;

namespace Orchard.CustomSettings.Services
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
