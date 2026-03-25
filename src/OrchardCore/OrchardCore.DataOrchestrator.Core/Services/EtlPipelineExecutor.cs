using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using OrchardCore.DataOrchestrator.Activities;
using OrchardCore.DataOrchestrator.Models;

namespace OrchardCore.DataOrchestrator.Services;

/// <summary>
/// Default implementation of <see cref="IEtlPipelineExecutor"/> that uses a stack-based
/// approach following activity transitions.
/// </summary>
public sealed class EtlPipelineExecutor : IEtlPipelineExecutor
{
    private readonly IEtlActivityLibrary _activityLibrary;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EtlPipelineExecutor> _logger;

    public EtlPipelineExecutor(
        IEtlActivityLibrary activityLibrary,
        IServiceProvider serviceProvider,
        ILogger<EtlPipelineExecutor> logger)
    {
        _activityLibrary = activityLibrary;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<EtlExecutionLog> ExecuteAsync(
        EtlPipelineDefinition pipeline,
        IDictionary<string, object> parameters = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pipeline);

        var log = new EtlExecutionLog
        {
            PipelineId = pipeline.PipelineId,
            PipelineName = pipeline.Name,
            StartedUtc = DateTime.UtcNow,
            Status = "Running",
        };

        try
        {
            _logger.LogInformation("Starting ETL pipeline '{PipelineName}' ({PipelineId}).", pipeline.Name, pipeline.PipelineId);

            var context = new EtlExecutionContext(pipeline, _activityLibrary, _serviceProvider, _logger, cancellationToken);

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    context.Parameters[param.Key] = param.Value;
                }
            }

            var startActivities = pipeline.Activities.Where(a => a.IsStart).ToList();

            if (startActivities.Count == 0)
            {
                throw new InvalidOperationException("Pipeline has no start activity.");
            }

            var scheduled = new Stack<EtlActivityRecord>();

            foreach (var start in startActivities)
            {
                scheduled.Push(start);
            }

            while (scheduled.Count > 0 && !cancellationToken.IsCancellationRequested)
            {
                var activityRecord = scheduled.Pop();
                var activity = _activityLibrary.InstantiateActivity(activityRecord.Name);

                if (activity == null)
                {
                    log.Errors.Add($"Activity '{activityRecord.Name}' not found.");
                    log.ErrorCount++;
                    continue;
                }

                activity.Properties = activityRecord.Properties?.DeepClone() as JsonObject ?? [];

                var result = await activity.ExecuteAsync(context);

                if (!result.IsSuccess)
                {
                    log.Errors.Add($"Activity '{activityRecord.Name}': {result.ErrorMessage}");
                    log.ErrorCount++;
                    continue;
                }

                foreach (var outcome in result.Outcomes)
                {
                    var transitions = pipeline.Transitions
                        .Where(t => t.SourceActivityId == activityRecord.ActivityId
                            && t.SourceOutcomeName == outcome);

                    foreach (var transition in transitions)
                    {
                        var next = pipeline.Activities
                            .FirstOrDefault(a => a.ActivityId == transition.DestinationActivityId);

                        if (next != null)
                        {
                            scheduled.Push(next);
                        }
                    }
                }
            }

            log.Status = cancellationToken.IsCancellationRequested ? "Cancelled" :
                         log.ErrorCount > 0 ? "Failed" : "Success";

            _logger.LogInformation(
                "ETL pipeline '{PipelineName}' completed with status {Status}.",
                pipeline.Name, log.Status);
        }
        catch (OperationCanceledException)
        {
            log.Status = "Cancelled";
            _logger.LogWarning("ETL pipeline '{PipelineName}' was cancelled.", pipeline.Name);
        }
        catch (Exception ex)
        {
            log.Status = "Failed";
            log.Errors.Add(ex.Message);
            log.ErrorCount++;
            _logger.LogError(ex, "ETL pipeline '{PipelineName}' failed with an unhandled error.", pipeline.Name);
        }
        finally
        {
            log.CompletedUtc = DateTime.UtcNow;
        }

        return log;
    }
}
