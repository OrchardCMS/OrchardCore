using OrchardCore.Data.Migration;
using OrchardCore.Jobs.Indexes;
using OrchardCore.Jobs.Models;
using YesSql.Sql;

namespace OrchardCore.Jobs.Migrations;

public sealed class JobsMigrations : DataMigration
{
    public async Task<int> CreateAsync()
    {
        await SchemaBuilder.CreateMapIndexTableAsync<JobRunIndex>(table => table
            .Column<string>(nameof(JobRunIndex.RunId), column => column.WithLength(JobRunIndex.MaxIdLength))
            .Column<string>(nameof(JobRunIndex.JobName), column => column.WithLength(JobRunIndex.MaxJobNameLength))
            .Column<string>(nameof(JobRunIndex.Status), column => column.WithLength(20))
            .Column<DateTime>(nameof(JobRunIndex.CreatedUtc))
            .Column<DateTime>(nameof(JobRunIndex.DueUtc))
            .Column<DateTime>(nameof(JobRunIndex.CompletedUtc), column => column.Nullable())
            .Column<DateTime>(nameof(JobRunIndex.HeartbeatUtc), column => column.Nullable())
            .Column<string>(nameof(JobRunIndex.NodeId), column => column.Nullable().WithLength(JobRunIndex.MaxKeyLength))
            .Column<string>(nameof(JobRunIndex.RequiredNodeId), column => column.Nullable().WithLength(JobRunIndex.MaxKeyLength))
            .Column<string>(nameof(JobRunIndex.CorrelationId), column => column.Nullable().WithLength(JobRunIndex.MaxKeyLength))
            .Column<string>(nameof(JobRunIndex.DeduplicationKey), column => column.Nullable().WithLength(JobRunIndex.MaxKeyLength))
            .Column<string>(nameof(JobRunIndex.ConcurrencyKey), column => column.Nullable().WithLength(JobRunIndex.MaxKeyLength))
            .Column<bool>(nameof(JobRunIndex.CancelInProgress))
            .Column<string>(nameof(JobRunIndex.ParentRunId), column => column.Nullable().WithLength(JobRunIndex.MaxIdLength))
            .Column<string>(nameof(JobRunIndex.ScheduleId), column => column.Nullable().WithLength(JobRunIndex.MaxKeyLength)),
            collection: JobRun.Collection);

        // Finds a run by its identifier.
        await SchemaBuilder.AlterIndexTableAsync<JobRunIndex>(table => table
            .CreateIndex("IDX_JobRunIndex_RunId", "DocumentId", nameof(JobRunIndex.RunId)),
            collection: JobRun.Collection);

        // Finds the runs that are due, abandoned or old, and lists the runs by status.
        await SchemaBuilder.AlterIndexTableAsync<JobRunIndex>(table => table
            .CreateIndex("IDX_JobRunIndex_Status", "DocumentId", nameof(JobRunIndex.Status), nameof(JobRunIndex.DueUtc), nameof(JobRunIndex.HeartbeatUtc), nameof(JobRunIndex.CompletedUtc)),
            collection: JobRun.Collection);

        // Lists the runs of a job, the most recent first.
        await SchemaBuilder.AlterIndexTableAsync<JobRunIndex>(table => table
            .CreateIndex("IDX_JobRunIndex_JobName", "DocumentId", nameof(JobRunIndex.JobName), nameof(JobRunIndex.CreatedUtc)),
            collection: JobRun.Collection);

        // Finds the runs that do the same work, to not queue it twice.
        await SchemaBuilder.AlterIndexTableAsync<JobRunIndex>(table => table
            .CreateIndex("IDX_JobRunIndex_DeduplicationKey", "DocumentId", nameof(JobRunIndex.DeduplicationKey)),
            collection: JobRun.Collection);

        // Finds the runs that execute one at a time.
        await SchemaBuilder.AlterIndexTableAsync<JobRunIndex>(table => table
            .CreateIndex("IDX_JobRunIndex_ConcurrencyKey", "DocumentId", nameof(JobRunIndex.ConcurrencyKey)),
            collection: JobRun.Collection);

        // Finds the child runs of a run.
        await SchemaBuilder.AlterIndexTableAsync<JobRunIndex>(table => table
            .CreateIndex("IDX_JobRunIndex_ParentRunId", "DocumentId", nameof(JobRunIndex.ParentRunId)),
            collection: JobRun.Collection);

        await SchemaBuilder.CreateMapIndexTableAsync<JobScheduleIndex>(table => table
            .Column<string>(nameof(JobScheduleIndex.ScheduleId), column => column.WithLength(JobRunIndex.MaxKeyLength))
            .Column<string>(nameof(JobScheduleIndex.JobName), column => column.WithLength(JobRunIndex.MaxJobNameLength))
            .Column<bool>(nameof(JobScheduleIndex.IsEnabled))
            .Column<DateTime>(nameof(JobScheduleIndex.NextOccurrenceUtc), column => column.Nullable()),
            collection: JobRun.Collection);

        await SchemaBuilder.AlterIndexTableAsync<JobScheduleIndex>(table => table
            .CreateIndex("IDX_JobScheduleIndex_DocumentId", "DocumentId", nameof(JobScheduleIndex.IsEnabled), nameof(JobScheduleIndex.NextOccurrenceUtc)),
            collection: JobRun.Collection);

        return 1;
    }
}
