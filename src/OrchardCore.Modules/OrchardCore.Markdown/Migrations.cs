using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrchardCore.Markdown.Fields;
using OrchardCore.Markdown.Settings;

namespace OrchardCore.Markdown
{
    public class Migrations : DataMigration
    {
        private IContentDefinitionManager _contentDefinitionManager;

        public Migrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public int Create()
        {
            _contentDefinitionManager.AlterPartDefinition("MarkdownBodyPart", builder => builder
                .Attachable()
                .WithDescription("Provides a Markdown formatted body for your content item."));

            // Return 2 to shortcut the third migration on new content definition schemas.
            return 2;
        }

        // Migrate FieldSettings. This only needs to run on old content definition schemas.
        // This code can be removed in a later version.
        public int UpdateFrom1()
        {
            _contentDefinitionManager.MigrateFieldSettings<MarkdownField, MarkdownFieldSettings>();
            return 2;
        }
    }
}
