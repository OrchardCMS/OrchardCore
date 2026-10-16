using Microsoft.Data.Sqlite;
using OrchardCore.Data.Migration;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Modules.OrchardCore.DataPipelines;

internal static class DataPipelineTestSites
{
    /// <summary>
    /// Waits until the tenant runs with the features a test enabled. Enabling features releases the tenant, which
    /// creates the tables of the features when it is created again. The SQLite connections pooled before keep the
    /// schema they read: a query on one of them while another connection writes, such as when a test starts several
    /// scopes at once, or a run writes in the background, fails with "no such table" as SQLite can't read the new
    /// schema. Clearing the pool of the tenant makes every connection read it.
    /// </summary>
    public static async Task WaitForFeaturesAsync(SiteContext context)
    {
        await context.UsingTenantScopeAsync(scope => scope.ServiceProvider.GetRequiredService<IDataMigrationManager>().UpdateAllFeaturesAsync());

        // The tasks the first scope of the new tenant deferred run when it ends: start the test after them.
        await context.UsingTenantScopeAsync(async scope =>
        {
            if (await scope.ServiceProvider.GetRequiredService<YesSql.ISession>().CreateConnectionAsync() is SqliteConnection connection)
            {
                SqliteConnection.ClearPool(connection);
            }
        });
    }
}
