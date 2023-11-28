using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Data.Migration;
using OrchardCore.Environment.Shell;
using OrchardCore.Media.Fields;
using OrchardCore.Media.Settings;

namespace OrchardCore.Media
{
    public class Migrations : DataMigration
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ShellSettings _shellSettings;

        public Migrations(IContentDefinitionManager contentDefinitionManager, ShellSettings shellSettings)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _shellSettings = shellSettings;
        }

        // This migration does not need to run on a new installation, but because there is no initial
        // migration record, the 'Create' migration is used with a tenant setup in progress checking.
        public async Task<int> CreateAsync()
        {
            if (_shellSettings.IsInitializing())
            {
                return 1;
            }

            await _contentDefinitionManager.MigrateFieldSettingsAsync<MediaField, MediaFieldSettings>();

            return 1;
        }
    }
}
