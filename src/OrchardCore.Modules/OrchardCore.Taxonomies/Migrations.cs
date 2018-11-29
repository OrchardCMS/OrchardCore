using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrchardCore.Taxonomies.Indexing;

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
                .WithPart("AliasPart", part => part.WithPosition("2").WithSettings(new AliasPartSettings { Pattern = "{{ ContentItem | display_text | slugify }}" }))
                .WithPart("TaxonomyPart", part => part.WithPosition("3"))
            );

            SchemaBuilder.CreateMapIndexTable(nameof(TaxonomyIndex), table => table
                .Column<string>("TaxonomyContentItemId", c => c.WithLength(26))
                .Column<string>("ContentItemId", c => c.WithLength(26))
                .Column<string>("ContentType", column => column.WithLength(255))
                .Column<string>("ContentPart", column => column.WithLength(255))
                .Column<string>("ContentField", column => column.WithLength(255))
                .Column<string>("TermContentItemId", column => column.WithLength(26))
            );

            SchemaBuilder.AlterTable(nameof(TaxonomyIndex), table => table
                .CreateIndex("IDX_TaxonomyIndex_List", "ContentType", "ContentPart", "ContentField")
            );

            SchemaBuilder.AlterTable(nameof(TaxonomyIndex), table => table
                .CreateIndex("IDX_TaxonomyIndex_Search", "TermContentItemId")
            );

            return 1;
        }
    }

    class AliasPartSettings
    {
        public string Pattern { get; set; }
    }
}
