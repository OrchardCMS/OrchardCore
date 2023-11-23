using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Data.Migration;
using OrchardCore.Media.Fields;
using OrchardCore.Media.Settings;

namespace OrchardCore.Media
{
    public class Migrations : DataMigration
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public Migrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        // This migration does not need to run on new installations, but because there is no
        // initial migration record, there is no way to shortcut the Create migration.
        public async Task<int> CreateAsync()
        {
            await _contentDefinitionManager.MigrateFieldSettingsAsync<MediaField, MediaFieldSettings>();

            return 1;
        }
    }
}
