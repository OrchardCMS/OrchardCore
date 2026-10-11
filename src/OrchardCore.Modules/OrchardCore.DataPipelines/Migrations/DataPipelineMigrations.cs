using OrchardCore.Data.Migration;
using OrchardCore.DataPipelines.Indexes;
using YesSql.Sql;

namespace OrchardCore.DataPipelines.Migrations;

public sealed class DataPipelineMigrations : DataMigration
{
    public async Task<int> CreateAsync()
    {
        await SchemaBuilder.CreateMapIndexTableAsync<DataPipelineIndex>(table => table
            .Column<string>("PipelineId", column => column.WithLength(DataPipelineIndex.MaxIdLength))
            .Column<string>("Name", column => column.WithLength(DataPipelineIndex.MaxNameLength))
            .Column<bool>("IsEnabled")
            .Column<bool>("IsPublished")
            .Column<DateTime>("ModifiedUtc")
        );

        await SchemaBuilder.AlterIndexTableAsync<DataPipelineIndex>(table => table
            .CreateIndex("IDX_DataPipelineIndex_DocumentId", "DocumentId", "PipelineId", "IsEnabled")
        );

        await SchemaBuilder.CreateMapIndexTableAsync<DataPipelineVersionIndex>(table => table
            .Column<string>("VersionId", column => column.WithLength(DataPipelineIndex.MaxIdLength))
            .Column<string>("PipelineId", column => column.WithLength(DataPipelineIndex.MaxIdLength))
            .Column<int>("Number")
            .Column<DateTime>("PublishedUtc")
        );

        await SchemaBuilder.AlterIndexTableAsync<DataPipelineVersionIndex>(table => table
            .CreateIndex("IDX_DataPipelineVersionIndex_DocumentId", "DocumentId", "PipelineId", "VersionId", "Number")
        );

        await SchemaBuilder.CreateMapIndexTableAsync<DataPipelineRunIndex>(table => table
            .Column<string>("RunId", column => column.WithLength(DataPipelineIndex.MaxIdLength))
            .Column<string>("PipelineId", column => column.WithLength(DataPipelineIndex.MaxIdLength))
            .Column<string>("Status", column => column.WithLength(20))
            .Column<string>("CorrelationId", column => column.Nullable().WithLength(DataPipelineRunIndex.MaxCorrelationIdLength))
            .Column<DateTime>("QueuedUtc")
            .Column<DateTime>("CompletedUtc", column => column.Nullable())
            .Column<DateTime>("HeartbeatUtc", column => column.Nullable())
        );

        await SchemaBuilder.AlterIndexTableAsync<DataPipelineRunIndex>(table => table
            .CreateIndex("IDX_DataPipelineRunIndex_DocumentId", "DocumentId", "PipelineId", "Status", "QueuedUtc")
        );

        await SchemaBuilder.AlterIndexTableAsync<DataPipelineRunIndex>(table => table
            .CreateIndex("IDX_DataPipelineRunIndex_RunId", "DocumentId", "RunId")
        );

        await SchemaBuilder.CreateMapIndexTableAsync<DataPipelineSharedFileIndex>(table => table
            .Column<string>("FileId", column => column.WithLength(DataPipelineIndex.MaxIdLength))
            .Column<string>("FileName", column => column.WithLength(DataPipelineSharedFileIndex.MaxFileNameLength))
            .Column<string>("PipelineId", column => column.WithLength(DataPipelineIndex.MaxIdLength))
            .Column<string>("RunId", column => column.WithLength(DataPipelineIndex.MaxIdLength))
            .Column<DateTime>("CreatedUtc")
            .Column<DateTime>("ExpiresUtc")
        );

        await SchemaBuilder.AlterIndexTableAsync<DataPipelineSharedFileIndex>(table => table
            .CreateIndex("IDX_DataPipelineSharedFileIndex_DocumentId", "DocumentId", "FileId", "ExpiresUtc")
        );

        await SchemaBuilder.CreateMapIndexTableAsync<DataPipelineSharedFileRecipientIndex>(table => table
            .Column<string>("FileId", column => column.WithLength(DataPipelineIndex.MaxIdLength))
            .Column<string>("UserId", column => column.WithLength(DataPipelineIndex.MaxIdLength))
            .Column<DateTime>("CreatedUtc")
            .Column<DateTime>("ExpiresUtc")
        );

        await SchemaBuilder.AlterIndexTableAsync<DataPipelineSharedFileRecipientIndex>(table => table
            .CreateIndex("IDX_DataPipelineSharedFileRecipientIndex_DocumentId", "DocumentId", "UserId", "ExpiresUtc")
        );

        return 1;
    }
}
