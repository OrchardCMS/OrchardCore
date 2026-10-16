using OrchardCore.Data.Migration;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Modules.OrchardCore.DataPipelines;

internal static class DataPipelineTestSites
{
    /// <summary>
    /// Waits until the tenant runs with the features a test enabled. Enabling features releases the tenant, which runs
    /// the migrations of the features when it is created again: a test that starts several scopes at once right after
    /// would race with that, and could query a table that doesn't exist yet.
    /// </summary>
    public static async Task WaitForFeaturesAsync(SiteContext context)
    {
        await context.UsingTenantScopeAsync(scope => scope.ServiceProvider.GetRequiredService<IDataMigrationManager>().UpdateAllFeaturesAsync());

        // The tasks the first scope of the new tenant deferred run when it ends: start the test after them.
        await context.UsingTenantScopeAsync(_ => Task.CompletedTask);
    }
}
