using System.Linq;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrchardCore.Markdown.Fields;
using OrchardCore.Markdown.Settings;

namespace OrchardCore.Markdown
{
    public class Migrations : DataMigration
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public Migrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public int Create()
        {
            _contentDefinitionManager.AlterPartDefinition("MarkdownBodyPart", builder => builder
                .Attachable()
                .WithDescription("Provides a Markdown formatted body for your content item."));

            // Shortcut other migration steps on new content definition schemas.
            return 4;
        }

        // Migrate FieldSettings. This only needs to run on old content definition schemas.
        // This code can be removed in a later version.
        public int UpdateFrom1()
        {
            _contentDefinitionManager.MigrateFieldSettings<MarkdownField, MarkdownFieldSettings>();
            return 2;
        }

        // This code can be removed in a later version.
        public int UpdateFrom2()
        {
            // For backwards compatability with liquid filters we disable html sanitization on existing field definitions.
            foreach (var contentType in _contentDefinitionManager.LoadTypeDefinitions())
            {
                if (contentType.Parts.Any(x => x.PartDefinition.Name == "MarkdownBodyPart"))
                {
                    _contentDefinitionManager.AlterTypeDefinition(contentType.Name, x => x.WithPart("MarkdownBodyPart", part =>
                    {
                        part.MergeSettings<MarkdownBodyPartSettings>(x => x.SanitizeHtml = false);
                    }));
                }
            }

            return 3;
        }

        // This code can be removed in a later version.
        public int UpdateFrom3()
        {
            // For backwards compatability with liquid filters we disable html sanitization on existing field definitions.
            var partDefinitions = _contentDefinitionManager.LoadPartDefinitions();
            foreach (var partDefinition in partDefinitions)
            {
                if (partDefinition.Fields.Any(x => x.FieldDefinition.Name == "MarkdownField"))
                {
                    _contentDefinitionManager.AlterPartDefinition(partDefinition.Name, partBuilder =>
                    {
                        foreach (var fieldDefinition in partDefinition.Fields.Where(x => x.FieldDefinition.Name == "MarkdownField"))
                        {
                            partBuilder.WithField(fieldDefinition.Name, fieldBuilder =>
                            {
                                fieldBuilder.MergeSettings<MarkdownFieldSettings>(s => s.SanitizeHtml = false);
                            });
                        }
                    });
                }
            }

            return 4;
        }
    }
}
