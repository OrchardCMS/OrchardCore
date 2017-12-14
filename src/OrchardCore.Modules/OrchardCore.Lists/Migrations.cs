using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Data.Migration;
using OrchardCore.Lists.Indexes;

namespace OrchardCore.Lists
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
            _contentDefinitionManager.AlterPartDefinition("ListPart", builder => builder
                .Attachable()
                .WithDescription("Add a list behavior."));

            SchemaBuilder.CreateMapIndexTable(nameof(ContainedPartIndex), table => table
                .Column<string>("ListContentItemId", c => c.WithLength(26))
                .Column<int>("Order")
            );

            SchemaBuilder.AlterTable(nameof(ContainedPartIndex), table => table
                .CreateIndex("IDX_ContainedPartIndex_ListContentItemId", "ListContentItemId")
            );

            return 1;
        }
    }
}