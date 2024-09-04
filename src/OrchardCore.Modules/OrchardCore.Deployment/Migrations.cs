using OrchardCore.Data.Migration;
using OrchardCore.Deployment.Indexes;
using YesSql.Sql;

namespace OrchardCore.Deployment;

public sealed class Migrations : DataMigration
{
    public async Task<int> CreateAsync()
    {
        await SchemaBuilder.CreateMapIndexTableAsync<DeploymentPlanIndex>(table => table
            .Column<string>("Name")
        );

        return 1;
    }
}
