using System.Diagnostics;
using OrchardCore.Environment.Shell;
using OrchardCore.Jobs;
using OrchardCore.Jobs.Models;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Tests.Modules.OrchardCore.DataPipelines;

namespace OrchardCore.Tests.Modules.OrchardCore.Jobs;

/// <summary>
/// What the tests of the features that queue jobs share: a site with features enabled, and waiting for a run.
/// </summary>
internal static class JobsTestSite
{
    /// <summary>
    /// Creates a site with the Jobs feature and the given features enabled.
    /// </summary>
    public static async Task<SiteContext> CreateAsync(params string[] featureIds)
    {
        var context = new SiteContext();
        await context.InitializeAsync();

        await context.UsingTenantScopeAsync(async scope =>
        {
            var featuresManager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            var features = (await featuresManager.GetAvailableFeaturesAsync())
                .Where(feature => feature.Id == "OrchardCore.Jobs" || featureIds.Contains(feature.Id))
                .ToList();

            await featuresManager.UpdateFeaturesAsync([], features, force: true);
        });
        await DataPipelineTestSites.WaitForFeaturesAsync(context);

        return context;
    }

    /// <summary>
    /// Waits for a run to end, and returns it.
    /// </summary>
    public static async Task<JobRun> WaitForRunAsync(SiteContext context, string runId)
    {
        Assert.NotNull(runId);

        var stopwatch = Stopwatch.StartNew();
        JobRun run = null;

        while (stopwatch.Elapsed < TimeSpan.FromSeconds(90))
        {
            await context.UsingTenantScopeAsync(async scope => run = await scope.ServiceProvider.GetRequiredService<IJobManager>().GetAsync(runId));

            if (run?.IsCompleted == true)
            {
                return run;
            }

            await Task.Delay(100);
        }

        throw new TimeoutException($"The run didn't end; its status is {run?.Status}, its error '{run?.Error}'.");
    }

    /// <summary>
    /// Waits for the most recent run of a job with a deduplication key to end, and returns it.
    /// </summary>
    public static async Task<JobRun> WaitForLastRunAsync(SiteContext context, string jobName, string deduplicationKey)
    {
        var stopwatch = Stopwatch.StartNew();
        JobRun run = null;

        while (stopwatch.Elapsed < TimeSpan.FromSeconds(90))
        {
            await context.UsingTenantScopeAsync(async scope => run = await scope.ServiceProvider.GetRequiredService<IJobManager>().FindLastAsync(jobName, deduplicationKey));

            if (run?.IsCompleted == true)
            {
                return run;
            }

            await Task.Delay(100);
        }

        throw new TimeoutException($"The run of '{jobName}' didn't end; its status is {run?.Status}, its error '{run?.Error}'.");
    }
}
