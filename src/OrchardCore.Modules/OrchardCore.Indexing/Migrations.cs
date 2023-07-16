using System;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.Environment.Shell;
using YesSql;

namespace OrchardCore.Indexing
{
    public class Migrations : DataMigration
    {
        private readonly ShellSettings _shellSettings;

        public Migrations(ShellSettings shellSettings) => _shellSettings = shellSettings;

        public int Create()
        {
            var identityColumnSize = Enum.Parse<IdentityColumnSize>(_shellSettings.GetIdentityColumnSize());

            SchemaBuilder.CreateTable(nameof(IndexingTask), table => table
                .Column(identityColumnSize, nameof(IndexingTask.Id), col => col.PrimaryKey().Identity())
                .Column<string>(nameof(IndexingTask.ContentItemId), c => c.WithLength(26))
                .Column<DateTime>(nameof(IndexingTask.CreatedUtc), col => col.NotNull())
                .Column<int>(nameof(IndexingTask.Type))
            );

            SchemaBuilder.AlterTable(nameof(IndexingTask), table => table
                .CreateIndex("IDX_IndexingTask_ContentItemId", "ContentItemId")
            );

            return 1;
        }
    }
}
