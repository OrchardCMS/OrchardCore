using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Data.Migration;

namespace OrchardCore.ContentFields
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

            // Youtube field
            await _contentDefinitionManager.MigrateFieldSettingsAsync<YoutubeField, YoutubeFieldSettings>();

            // Shortcut other migration steps on new content definition schemas.
            return 2;
        }

        // This code can be removed in a later version.
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
