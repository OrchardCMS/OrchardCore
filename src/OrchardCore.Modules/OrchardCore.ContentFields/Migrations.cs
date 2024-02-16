using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Data.Migration;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Descriptor.Models;

namespace OrchardCore.ContentFields
{
    public class Migrations : DataMigration
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ShellDescriptor _shellDescriptor;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public Migrations(
            IContentDefinitionManager contentDefinitionManager,
            ShellDescriptor shellDescriptor,
            IOptions<JsonSerializerOptions> jsonSerializerOptions)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _shellDescriptor = shellDescriptor;
            _jsonSerializerOptions = jsonSerializerOptions.Value;
        }

        // New installations don't need to be upgraded, but because there is no initial migration record,
        // 'UpgradeAsync' is called in a new 'CreateAsync' but only if the feature was already installed.
        public async Task<int> CreateAsync()
        {
            if (_shellDescriptor.WasFeatureAlreadyInstalled("OrchardCore.ContentFields"))
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
            await _contentDefinitionManager.MigrateFieldSettingsAsync<BooleanField, BooleanFieldSettings>(_jsonSerializerOptions);

            // Content picker field
            await _contentDefinitionManager.MigrateFieldSettingsAsync<ContentPickerField, ContentPickerFieldSettings>(_jsonSerializerOptions);

            // Date field
            await _contentDefinitionManager.MigrateFieldSettingsAsync<DateField, DateFieldSettings>(_jsonSerializerOptions);

            // Date time field
            await _contentDefinitionManager.MigrateFieldSettingsAsync<DateTimeField, DateTimeFieldSettings>(_jsonSerializerOptions);

            // Html field
            await _contentDefinitionManager.MigrateFieldSettingsAsync<HtmlField, HtmlFieldSettings>(_jsonSerializerOptions);

            // Link field
            await _contentDefinitionManager.MigrateFieldSettingsAsync<LinkField, LinkFieldSettings>(_jsonSerializerOptions);

            // Localization set content picker field
            await _contentDefinitionManager.MigrateFieldSettingsAsync<LocalizationSetContentPickerField, LocalizationSetContentPickerFieldSettings>(_jsonSerializerOptions);

            // MultiText field
            await _contentDefinitionManager.MigrateFieldSettingsAsync<MultiTextField, MultiTextFieldSettings>(_jsonSerializerOptions);

            // Numeric field
            await _contentDefinitionManager.MigrateFieldSettingsAsync<NumericField, NumericFieldSettings>(_jsonSerializerOptions);

            // Text field
            await _contentDefinitionManager.MigrateFieldSettingsAsync<TextField, TextFieldHeaderDisplaySettings>(_jsonSerializerOptions);
            await _contentDefinitionManager.MigrateFieldSettingsAsync<TextField, TextFieldPredefinedListEditorSettings>(_jsonSerializerOptions);
            await _contentDefinitionManager.MigrateFieldSettingsAsync<TextField, TextFieldSettings>(_jsonSerializerOptions);

            // Time field
            await _contentDefinitionManager.MigrateFieldSettingsAsync<TimeField, TimeFieldSettings>(_jsonSerializerOptions);

            // YouTube field
            await _contentDefinitionManager.MigrateFieldSettingsAsync<YoutubeField, YoutubeFieldSettings>(_jsonSerializerOptions);

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
