using System;
using OrchardCore.BackgroundJobs.Indexes;
using OrchardCore.BackgroundJobs.Models;
using OrchardCore.Data.Migration;
using YesSql.Sql;

namespace OrchardCore.BackgroundJobs
{
    public class Migrations : DataMigration
    {
        public int Create()
        {
            SchemaBuilder.CreateMapIndexTable<BackgroundJobIndex>(table => table
                .Column<string>(nameof(BackgroundJobIndex.BackgroundJobId), c => c.WithLength(26))
                .Column<string>(nameof(BackgroundJobIndex.Name), c => c.WithLength(255))
                .Column<string>(nameof(BackgroundJobIndex.RepeatCorrelationId), c => c.WithLength(26))
                .Column<string>(nameof(BackgroundJobIndex.CorrelationId), c => c.WithLength(26))
                .Column<DateTime>(nameof(BackgroundJobIndex.CreatedUtc), c => c.Nullable())
                .Column<BackgroundJobStatus>(nameof(BackgroundJobIndex.Status))
                .Column<DateTime>(nameof(BackgroundJobIndex.ExecutionUtc), c => c.Nullable())
            );

            return 1;
        }
    }
}
