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
        IContentDefinitionManager _contentDefinitionManager;

        public Migrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public int Create()
        {
            _contentDefinitionManager.AlterTypeDefinition("Taxonomy", menu => menu
                .Draftable()
                .Versionable()
                .Creatable()
                .Listable()
                .WithPart("TitlePart", part => part.WithPosition("1"))
                .WithPart("AliasPart", part => part.WithPosition("2").WithSettings(new AliasPartSettings { Pattern = "{{ Model.ContentItem | display_text | slugify }}" }))
                .WithPart("TaxonomyPart", part => part.WithPosition("3"))
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

            // Return 2 to shortcut the second migration on new content definition schemas.
            return 1;
        }

        // Migrate FieldSettings. This only needs to run on old content definition schemas.
        // This code can be removed in a later version.
        public int UpdateFrom1()
        {
            _contentDefinitionManager.MigrateFieldSettings<TaxonomyField, TaxonomyFieldSettings>();
            return 2;
        }
    }

    class AliasPartSettings
    {
        public string Pattern { get; set; }
    }
}
