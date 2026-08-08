using OrchardCore.Data.Migration;
using OrchardCore.DataOrchestrator.Indexes;
using YesSql.Sql;

namespace OrchardCore.DataOrchestrator.Migrations;

public sealed class DataOrchestratorIndexMigrations : DataMigration
{
    public async Task<int> CreateAsync()
    {
        await SchemaBuilder.CreateMapIndexTableAsync<EtlPipelineIndex>(table => table
            .Column<string>("PipelineId", col => col.WithLength(26))
            .Column<string>("Name", col => col.WithLength(255))
            .Column<bool>("IsEnabled")
        );

        await SchemaBuilder.AlterIndexTableAsync<EtlPipelineIndex>(table => table
            .CreateIndex("IDX_EtlPipelineIndex_DocumentId",
                "DocumentId",
                "PipelineId",
                "IsEnabled")
        );

        await SchemaBuilder.CreateMapIndexTableAsync<EtlExecutionLogIndex>(table => table
            .Column<string>("PipelineId", col => col.WithLength(44))
            .Column<DateTime>("StartedUtc")
            .Column<string>("Status", col => col.WithLength(20))
        );

        await SchemaBuilder.AlterIndexTableAsync<EtlExecutionLogIndex>(table => table
            .CreateIndex("IDX_EtlExecutionLogIndex_DocumentId",
                "DocumentId",
                "PipelineId",
                "StartedUtc")
        );

        return 1;
    }
}
