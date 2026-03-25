using OrchardCore.BackgroundTasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.DataOrchestrator.Services;

namespace OrchardCore.DataOrchestrator.BackgroundTasks;

[BackgroundTask(
    Schedule = "*/10 * * * *",
    Title = "Data Pipeline Execution",
    Description = "Executes enabled data pipelines on their configured schedule.")]
public sealed class EtlPipelineBackgroundTask : IBackgroundTask
{
    public async Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var pipelineService = serviceProvider.GetRequiredService<IEtlPipelineService>();
        var executor = serviceProvider.GetRequiredService<IEtlPipelineExecutor>();
        var logger = serviceProvider.GetRequiredService<ILogger<EtlPipelineBackgroundTask>>();

        var pipelines = await pipelineService.ListEnabledAsync();

        foreach (var pipeline in pipelines)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            if (string.IsNullOrEmpty(pipeline.Schedule))
            {
                continue;
            }

            try
            {
                var log = await executor.ExecuteAsync(pipeline, cancellationToken: cancellationToken);
                await pipelineService.SaveLogAsync(log);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error executing ETL pipeline '{PipelineName}'.", pipeline.Name);
            }
        }
    }
}
