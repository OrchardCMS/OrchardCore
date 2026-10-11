using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.DataPipelines.Models;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.DataPipelines.Services;

/// <summary>
/// Tells the <see cref="IDataPipelineRunHandler"/> services that a run ended, such as to resume the workflow that waits
/// for it, whether the run ended by itself, was cancelled before it started, or stopped reporting its progress.
/// </summary>
internal static class DataPipelineRunNotifications
{
    /// <summary>
    /// Calls the handlers. A failing handler is logged, and doesn't stop the others.
    /// </summary>
    /// <param name="services">The services of the scope the handlers run in.</param>
    /// <param name="run">The run that ended.</param>
    /// <returns>A task that completes when every handler was called.</returns>
    public static async Task NotifyAsync(IServiceProvider services, DataPipelineRun run)
    {
        foreach (var handler in services.GetServices<IDataPipelineRunHandler>())
        {
            try
            {
                await handler.CompletedAsync(run);
            }
            catch (Exception ex)
            {
                services.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(DataPipelineRunNotifications))
                    .LogError(ex, "A handler of the end of the data pipeline run '{RunId}' failed.", run.RunId);
            }
        }
    }

    /// <summary>
    /// Calls the handlers once the current scope ends, so after the session that saved the end of the run commits.
    /// </summary>
    /// <param name="run">The run that ended.</param>
    public static void NotifyAfterCommit(DataPipelineRun run)
        => ShellScope.AddDeferredTask(scope => NotifyAsync(scope.ServiceProvider, run));
}
