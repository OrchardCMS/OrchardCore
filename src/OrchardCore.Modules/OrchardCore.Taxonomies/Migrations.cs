using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Data.Migration;
using OrchardCore.Taxonomies.Fields;
using OrchardCore.Taxonomies.Indexing;
using OrchardCore.Taxonomies.Settings;

namespace OrchardCore.Taxonomies
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
            _contentDefinitionManager.AlterTypeDefinition("Taxonomy", taxonomy => taxonomy
                .Draftable()
                .Versionable()
                .Creatable()
                .Listable()
                .WithPart("TitlePart", part => part.WithPosition("1"))
                .WithPart("AliasPart", part => part
                    .WithPosition("2")
                    .WithSettings(new AliasPartSettings
                    {
                        Pattern = "{{ Model.ContentItem | display_text | slugify }}"
                    }))
                .WithPart("AutoroutePart", part => part
                    .WithPosition("3")
                    .WithSettings(new AutoroutePartSettings
                    {
                        Pattern = "{{ Model.ContentItem | display_text | slugify }}",
                        AllowRouteContainedItems = true
                    }))
                .WithPart("TaxonomyPart", part => part.WithPosition("4"))
            );

            SchemaBuilder.CreateMapIndexTable(nameof(TaxonomyIndex), table => table
                .Column<string>("TaxonomyContentItemId", c => c.WithLength(26))
                .Column<string>("ContentItemId", c => c.WithLength(26))
                .Column<string>("ContentType", column => column.WithLength(ContentItemIndex.MaxContentTypeSize))
                .Column<string>("ContentPart", column => column.WithLength(ContentItemIndex.MaxContentPartSize))
                .Column<string>("ContentField", column => column.WithLength(ContentItemIndex.MaxContentFieldSize))
                .Column<string>("TermContentItemId", column => column.WithLength(26))
            );

            SchemaBuilder.AlterTable(nameof(TaxonomyIndex), table => table
                .CreateIndex("IDX_TaxonomyIndex_List", "ContentType", "ContentPart", "ContentField")
            );

            SchemaBuilder.AlterTable(nameof(TaxonomyIndex), table => table
                .CreateIndex("IDX_TaxonomyIndex_Search", "TermContentItemId")
            );

            // Return 3 to shortcut the migrations on new content definition schemas.
            return 3;
        }

        // Migrate FieldSettings. This only needs to run on old content definition schemas.
        // This code can be removed in a later version.
        public int UpdateFrom1()
        {
            _contentDefinitionManager.MigrateFieldSettings<TaxonomyField, TaxonomyFieldSettings>();
            return 2;
        }

        public int UpdateFrom2()
        {
            _contentDefinitionManager.AlterTypeDefinition("Taxonomy", taxonomy => taxonomy
                .WithPart("AutoroutePart", part => part
                    .WithPosition("3")
                    .WithSettings(new AutoroutePartSettings
                    {
                        Pattern = "{{ Model.ContentItem | display_text | slugify }}",
                        AllowRouteContainedItems = true
                    }))
                .WithPart("TaxonomyPart", part => part.WithPosition("4"))
            );

            return 3;
        }
    }

    internal class AliasPartSettings
    {
        public string Pattern { get; set; }
    }

    internal class AutoroutePartSettings
    {
        public string Pattern { get; set; }
        public bool AllowRouteContainedItems { get; set; }
    }
}
