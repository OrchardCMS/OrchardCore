using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrchardCore.Lists.Indexes;
using OrchardCore.Lists.Models;

namespace OrchardCore.Lists
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

            // Return 2 to shortcut the second migration on new content definition schemas.
            return 2;
        }

        // Migrate PartSettings. This only needs to run on old content definition schemas.
        // This code can be removed in a later version.
        public int UpdateFrom1()
        {
            _contentDefinitionManager.MigratePartSettings<ListPart, ListPartSettings>();

            return 2;
        }
    }
}
