using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Data.Migration;
using OrchardCore.Markdown.Fields;
using OrchardCore.Markdown.Settings;

namespace OrchardCore.Markdown
{
    public class Migrations : DataMigration
    {
        IContentDefinitionManager _contentDefinitionManager;

        public Migrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public int Create()
        {
            _contentDefinitionManager.AlterPartDefinition("MarkdownBodyPart", builder => builder
                .Attachable()
                .WithDescription("Provides a Markdown formatted body for your content item."));

            //TODO shortcut
            return 1;
        }

        public int UpdateFrom1()
        {
            _contentDefinitionManager.MigrateFieldSettings<MarkdownField, MarkdownFieldSettings>();
            return 2;
        }
    }
}