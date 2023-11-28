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

        // New installations don't need to be upgraded, but because there is no initial migration record,
        // 'UpgradeAsync()' is called from 'CreateAsync()' and only if a tenant setup is not in progress.
        public async Task<int> CreateAsync()
        {
            if (!_shellSettings.IsInitializing())
            {
                await UpgradeAsync();
            }

            // Shortcut other migration steps on new content definition schemas.
            return 1;
        }

        // Upgrade an existing installation.
        private async Task UpgradeAsync()
        {
            await _contentDefinitionManager.MigrateFieldSettingsAsync<MediaField, MediaFieldSettings>();
        }
    }
}
