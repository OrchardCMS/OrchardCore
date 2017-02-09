using Orchard.Data.Migration;
using System;

namespace Orchard.ContentManagement.Records
{
    public class Migrations : DataMigration
    {
        public int Create()
        {
            SchemaBuilder.CreateMapIndexTable(nameof(ContentItemIndex), table => table
                .Column<string>("ContentItemId", c => c.WithLength(26))
                .Column<bool>("Latest")
                .Column<int>("Number")
                .Column<bool>("Published")
                .Column<string>("ContentType", column => column.WithLength(255))
                .Column<DateTime>("ModifiedUtc", column => column.Nullable())
                .Column<DateTime>("PublishedUtc", column => column.Nullable())
                .Column<DateTime>("CreatedUtc", column => column.Nullable())
                .Column<string>("Owner", column => column.WithLength(255).Nullable())
                .Column<string>("Author", column => column.WithLength(255).Nullable())
            );

            SchemaBuilder.AlterTable(nameof(ContentItemIndex), table => table
                .CreateIndex("IDX_ContentItemIndex_ContentItemId", "ContentItemId", "Latest", "Published", "CreatedUtc")
            );

            return 1;
        }
    }
}