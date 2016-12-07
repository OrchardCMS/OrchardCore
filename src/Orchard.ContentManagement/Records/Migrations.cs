using Orchard.Data.Migration;
using System;

namespace Orchard.ContentManagement.Records
{
    public class Migrations : DataMigration
    {
        public int Create()
        {
            SchemaBuilder.CreateMapIndexTable(nameof(ContentItemIndex), table => table
                .Column<string>("ContentItemId", c => c.WithLength(32))
                .Column<int>("Latest")
                .Column<int>("Number")
                .Column<int>("Published")
                .Column<string>("ContentType", column => column.WithLength(255))
                .Column<DateTime>("ModifiedUtc", column => column.WithLength(255).Nullable())
                .Column<DateTime>("PublishedUtc", column => column.WithLength(255).Nullable())
                .Column<DateTime>("CreatedUtc", column => column.WithLength(255).Nullable())
            );

            return 1;
        }
    }
}