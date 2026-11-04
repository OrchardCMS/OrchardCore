using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundTasks;
using OrchardCore.Deployment.Operations;

namespace OrchardCore.Deployment.Artifacts;

/// <summary>Removes expired artifacts, abandoned uploads and ended operations from the current tenant.</summary>
[BackgroundTask(Schedule = "*/15 * * * *", Description = "Deployment artifact and operation retention cleanup.")]
public sealed class DeploymentArtifactCleanupTask : IBackgroundTask
{
    /// <summary>Runs a bounded cleanup batch while preserving active artifact leases.</summary>
    public async Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        await serviceProvider.GetRequiredService<DeploymentArtifactStore>().CleanupAsync(cancellationToken: cancellationToken);
        await serviceProvider.GetRequiredService<DeploymentOperationStore>().PruneAsync(cancellationToken: cancellationToken);
    }
}
