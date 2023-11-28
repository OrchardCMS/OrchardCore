using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Data.Migration;
using OrchardCore.Environment.Shell;

namespace OrchardCore.ContentFields
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
            return 2;
        }

        // Upgrade an existing installation.
        private async Task UpgradeAsync()
        {
            // Boolean field
            await _contentDefinitionManager.MigrateFieldSettingsAsync<BooleanField, BooleanFieldSettings>();

            // Content picker field
            await _contentDefinitionManager.MigrateFieldSettingsAsync<ContentPickerField, ContentPickerFieldSettings>();

            // Date field
            await _contentDefinitionManager.MigrateFieldSettingsAsync<DateField, DateFieldSettings>();

            // Date time field
            await _contentDefinitionManager.MigrateFieldSettingsAsync<DateTimeField, DateTimeFieldSettings>();

            // Html field
            await _contentDefinitionManager.MigrateFieldSettingsAsync<HtmlField, HtmlFieldSettings>();

            // Link field
            await _contentDefinitionManager.MigrateFieldSettingsAsync<LinkField, LinkFieldSettings>();

            // Localization set content picker field
            await _contentDefinitionManager.MigrateFieldSettingsAsync<LocalizationSetContentPickerField, LocalizationSetContentPickerFieldSettings>();

            // MultiText field
            await _contentDefinitionManager.MigrateFieldSettingsAsync<MultiTextField, MultiTextFieldSettings>();

            // Numeric field
            await _contentDefinitionManager.MigrateFieldSettingsAsync<NumericField, NumericFieldSettings>();

            // Text field
            await _contentDefinitionManager.MigrateFieldSettingsAsync<TextField, TextFieldHeaderDisplaySettings>();
            await _contentDefinitionManager.MigrateFieldSettingsAsync<TextField, TextFieldPredefinedListEditorSettings>();
            await _contentDefinitionManager.MigrateFieldSettingsAsync<TextField, TextFieldSettings>();

            // Time field
            await _contentDefinitionManager.MigrateFieldSettingsAsync<TimeField, TimeFieldSettings>();

            // YouTube field
            await _contentDefinitionManager.MigrateFieldSettingsAsync<YoutubeField, YoutubeFieldSettings>();

            // Keep in sync the upgrade process.
            await UpdateFrom1Async();
        }

        // This step is part of the upgrade process.
        public async Task<int> UpdateFrom1Async()
        {
            // For backwards compatibility with liquid filters we disable html sanitization on existing field definitions.
            var partDefinitions = await _contentDefinitionManager.LoadPartDefinitionsAsync();
            foreach (var partDefinition in partDefinitions)
            {
                if (partDefinition.Fields.Any(x => x.FieldDefinition.Name == "HtmlField"))
                {
                    await _contentDefinitionManager.AlterPartDefinitionAsync(partDefinition.Name, partBuilder =>
                    {
                        foreach (var fieldDefinition in partDefinition.Fields.Where(x => x.FieldDefinition.Name == "HtmlField"))
                        {
                            partBuilder.WithField(fieldDefinition.Name, fieldBuilder =>
                            {
                                fieldBuilder.MergeSettings<HtmlFieldSettings>(x => x.SanitizeHtml = false);
                            });
                        }
                    });
                }
            }

            return 2;
        }
    }
}
