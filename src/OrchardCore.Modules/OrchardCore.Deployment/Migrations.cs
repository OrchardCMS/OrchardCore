using OrchardCore.Data.Migration;
using OrchardCore.Deployment.Indexes;
using OrchardCore.Deployment.Services;
using YesSql;
using YesSql.Sql;

namespace OrchardCore.Deployment;

public sealed class Migrations : DataMigration
{
    private readonly ISession _session;

    /// <summary>Creates deployment migrations for the tenant's persisted plans.</summary>
    public Migrations(ISession session)
    {
        _session = session;
    }

    public async Task<int> CreateAsync()
    {
        await SchemaBuilder.CreateMapIndexTableAsync<DeploymentPlanIndex>(table => table
            .Column<string>("Name")
        );

        return 2;
    }
    /// <summary>Assigns stable, unique IDs to legacy steps while preserving their configuration.</summary>
    public async Task<int> UpdateFrom1Async()
    {
        var plans = await _session.Query<DeploymentPlan, DeploymentPlanIndex>().ListAsync();
        foreach (var plan in plans)
        {
            if (DeploymentStepIdentities.EnsureUnique(plan.DeploymentSteps))
            {
                await _session.SaveAsync(plan);
            }
        }
        return 2;
    }
}
