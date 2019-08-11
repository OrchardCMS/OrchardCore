using OrchardCore.Data.Migration;
using System;

namespace OrchardCore.ContentManagement.Records
{
    public class Migrations : DataMigration
    {
        public int Create()
        {
            SchemaBuilder.CreateMapIndexTable(nameof(ContentItemIndex), table => table
                .Column<string>("ContentItemId", c => c.WithLength(26))
                .Column<string>("ContentItemVersionId", c => c.WithLength(26))
                .Column<bool>("Latest")
                .Column<bool>("Published")
                .Column<string>("ContentType", column => column.WithLength(ContentItemIndex.MaxContentTypeSize))
                .Column<DateTime>("ModifiedUtc", column => column.Nullable())
                .Column<DateTime>("PublishedUtc", column => column.Nullable())
                .Column<DateTime>("CreatedUtc", column => column.Nullable())
                .Column<string>("Owner", column => column.Nullable().WithLength(ContentItemIndex.MaxOwnerSize))
                .Column<string>("Author", column => column.Nullable().WithLength(ContentItemIndex.MaxAuthorSize))
                .Column<string>("DisplayText", column => column.Nullable().WithLength(ContentItemIndex.MaxDisplayTextSize))
            );

            SchemaBuilder.AlterTable(nameof(ContentItemIndex), table => table
                .CreateIndex("IDX_ContentItemIndex_ContentItemId", "ContentItemId", "Latest", "Published", "CreatedUtc")
            );

            SchemaBuilder.AlterTable(nameof(ContentItemIndex), table => table
                .CreateIndex("IDX_ContentItemIndex_ContentItemVersionId", "ContentItemVersionId")
            );

            SchemaBuilder.AlterTable(nameof(ContentItemIndex), table => table
                .CreateIndex("IDX_ContentItemIndex_DisplayText", "DisplayText")
            );

            return 3;
        }

        public int UpdateFrom1()
        {
            SchemaBuilder.AlterTable(nameof(ContentItemIndex), table => table
                .AddColumn<string>("ContentItemVersionId", c => c.WithLength(26))
            );

            SchemaBuilder.AlterTable(nameof(ContentItemIndex), table => table
                .CreateIndex("IDX_ContentItemIndex_ContentItemVersionId", "ContentItemVersionId")
            );

            return 2;
        }

        public int UpdateFrom2()
        {
            SchemaBuilder.AlterTable(nameof(ContentItemIndex), table => table
                .AddColumn<string>("DisplayText", column => column.Nullable().WithLength(ContentItemIndex.MaxDisplayTextSize))
            );

            SchemaBuilder.AlterTable(nameof(ContentItemIndex), table => table
                .CreateIndex("IDX_ContentItemIndex_DisplayText", "DisplayText")
            );

            return 3;
        }
    }
}