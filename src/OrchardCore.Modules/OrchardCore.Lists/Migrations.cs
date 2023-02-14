using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrchardCore.Lists.Indexes;
using OrchardCore.Lists.Models;
using YesSql.Sql;

namespace OrchardCore.Lists
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
            _contentDefinitionManager.AlterPartDefinition("ListPart", builder => builder
                .Attachable()
                .WithDescription("Add a list behavior."));

            SchemaBuilder.CreateMapIndexTable<ContainedPartIndex>(table => table
                .Column<string>("ContentItemId", column => column.WithLength(26))
                .Column<string>("ListContentItemId", column => column.WithLength(26))
                .Column<string>("DisplayText")
                .Column<int>("Order")
                .Column<string>("ListContentType")
                .Column<bool>("Published")
                .Column<bool>("Latest")

            );

            SchemaBuilder.AlterIndexTable<ContainedPartIndex>(table => table
                .CreateIndex("IDX_ContainedPartIndex_DocumentId",
                    "Id",
                    "DocumentId",
                    "ContentItemId",
                    "ListContentItemId",
                    "DisplayText",
                    "Order",
                    "ListContentType",
                    "Published",
                    "Latest")
            );

            // Shortcut other migration steps on new content definition schemas.
            return 4;
        }

        // Migrate PartSettings. This only needs to run on old content definition schemas.
        // This code can be removed in a later version.
        public int UpdateFrom1()
        {
            _contentDefinitionManager.MigratePartSettings<ListPart, ListPartSettings>();

            return 2;
        }

        // This code can be removed in a later version.
        public int UpdateFrom2()
        {
            SchemaBuilder.AlterIndexTable<ContainedPartIndex>(table => table
                .CreateIndex("IDX_ContainedPartIndex_DocumentId", "DocumentId", "ListContentItemId", "Order")
            );

            return 3;
        }

        // This code can be removed in a later version.
        public int UpdateFrom3()
        {
            SchemaBuilder.AlterIndexTable<ContainedPartIndex>(table => table
                .AddColumn<string>("ContentItemId", column => column.WithLength(26))
            );

            SchemaBuilder.AlterIndexTable<ContainedPartIndex>(table => table
                .AddColumn<string>("ListContentType")
            );

            SchemaBuilder.AlterIndexTable<ContainedPartIndex>(table => table
                .AddColumn<string>("DisplayText")
            );

            SchemaBuilder.AlterIndexTable<ContainedPartIndex>(table => table
                .AddColumn<bool>("Published")
            );

            SchemaBuilder.AlterIndexTable<ContainedPartIndex>(table => table
                .AddColumn<bool>("Latest")
            );
            SchemaBuilder.AlterIndexTable<ContainedPartIndex>(table => table
                .DropIndex("IDX_ContainedPartIndex_DocumentId")
            );

            SchemaBuilder.AlterIndexTable<ContainedPartIndex>(table => table
                .CreateIndex("IDX_ContainedPartIndex_DocumentId",
                    "Id",
                    "DocumentId",
                    "ContentItemId",
                    "ListContentItemId",
                    "DisplayText",
                    "Order",
                    "ListContentType",
                    "Published",
                    "Latest")
            );

            return 4;
        }
    }
}
