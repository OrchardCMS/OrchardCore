using OrchardCore.BackgroundTasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NCrontab;
using OrchardCore.DataOrchestrator.Models;
using OrchardCore.DataOrchestrator.Services;
using OrchardCore.Modules;

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
        var clock = serviceProvider.GetRequiredService<IClock>();
        var logger = serviceProvider.GetRequiredService<ILogger<EtlPipelineBackgroundTask>>();

        var pipelines = await pipelineService.ListEnabledAsync();
        var nowUtc = clock.UtcNow;

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

            if (!IsDue(pipeline, nowUtc, logger))
            {
                continue;
            }

            try
            {
                var log = await executor.ExecuteAsync(pipeline, cancellationToken: cancellationToken);
                await pipelineService.SaveLogAsync(log);

                pipeline.LastRunUtc = log.CompletedUtc ?? nowUtc;
                await pipelineService.SaveAsync(pipeline);
            }
            catch (Exception ex)
            {
                if (logger.IsEnabled(LogLevel.Error))
                {
                    logger.LogError(ex, "Error executing ETL pipeline '{PipelineName}'.", pipeline.Name);
                }
            }
        }
    }

    private static bool IsDue(EtlPipelineDefinition pipeline, DateTime nowUtc, ILogger logger)
    {
        CrontabSchedule schedule;

        try
        {
            schedule = CrontabSchedule.Parse(pipeline.Schedule);
        }
        catch (Exception ex) when (!ex.IsFatal())
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning(ex, "The ETL pipeline '{PipelineName}' has an invalid schedule '{Schedule}'.", pipeline.Name, pipeline.Schedule);
            }

            return false;
        }

        if (!pipeline.LastRunUtc.HasValue)
        {
            return true;
        }

        return schedule.GetNextOccurrence(pipeline.LastRunUtc.Value) <= nowUtc;
    }
}
