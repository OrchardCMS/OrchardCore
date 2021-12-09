using System.Linq;
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
        public int Create()
        {
            // Boolean field
            _contentDefinitionManager.MigrateFieldSettings<BooleanField, BooleanFieldSettings>();

            // Content picker field
            _contentDefinitionManager.MigrateFieldSettings<ContentPickerField, ContentPickerFieldSettings>();

            // Date field
            _contentDefinitionManager.MigrateFieldSettings<DateField, DateFieldSettings>();

            // Date time field
            _contentDefinitionManager.MigrateFieldSettings<DateTimeField, DateTimeFieldSettings>();

            // Html field
            _contentDefinitionManager.MigrateFieldSettings<HtmlField, HtmlFieldSettings>();

            // Link field
            _contentDefinitionManager.MigrateFieldSettings<LinkField, LinkFieldSettings>();

            // Localization set content picker field
            _contentDefinitionManager.MigrateFieldSettings<LocalizationSetContentPickerField, LocalizationSetContentPickerFieldSettings>();

            // MultiText field
            _contentDefinitionManager.MigrateFieldSettings<MultiTextField, MultiTextFieldSettings>();

            // Numeric field
            _contentDefinitionManager.MigrateFieldSettings<NumericField, NumericFieldSettings>();

            // Text field
            _contentDefinitionManager.MigrateFieldSettings<TextField, TextFieldHeaderDisplaySettings>();
            _contentDefinitionManager.MigrateFieldSettings<TextField, TextFieldPredefinedListEditorSettings>();
            _contentDefinitionManager.MigrateFieldSettings<TextField, TextFieldSettings>();

            // Time field
            _contentDefinitionManager.MigrateFieldSettings<TimeField, TimeFieldSettings>();

            // Youtube field
            _contentDefinitionManager.MigrateFieldSettings<YoutubeField, YoutubeFieldSettings>();

            // Shortcut other migration steps on new content definition schemas.
            return 2;
        }

        // This code can be removed in a later version.
        public int UpdateFrom1()
        {
            // For backwards compatability with liquid filters we disable html sanitization on existing field definitions.
            var partDefinitions = _contentDefinitionManager.LoadPartDefinitions();
            foreach (var partDefinition in partDefinitions)
            {
                if (partDefinition.Fields.Any(x => x.FieldDefinition.Name == "HtmlField"))
                {
                    _contentDefinitionManager.AlterPartDefinition(partDefinition.Name, partBuilder =>
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
