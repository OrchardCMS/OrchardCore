using System;
using Orchard.Data.Migration;

namespace Orchard.Indexing
{
    public class Migrations : DataMigration
    {
        public int Create()
        {
            SchemaBuilder.CreateTable(nameof(IndexingTask), table => table
                .Column<int>(nameof(IndexingTask.Id), col => col.PrimaryKey().Identity())
                .Column<string>(nameof(IndexingTask.ContentItemId), c => c.WithLength(32))
                .Column<DateTimeOffset>(nameof(IndexingTask.CreatedUtc), col => col.NotNull())
                .Column<int>(nameof(IndexingTask.Type))
            );

            // TODO: Add an index on ContentItemId as this is used in a where clause

            return 1;
        }
    }
}
