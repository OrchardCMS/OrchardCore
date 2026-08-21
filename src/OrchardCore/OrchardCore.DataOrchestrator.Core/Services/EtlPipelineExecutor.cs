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

        EtlExecutionContext context = null;

        try
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Starting ETL pipeline '{PipelineName}' ({PipelineId}).", pipeline.Name, pipeline.PipelineId);
            }

            var activities = pipeline.Activities ?? [];
            var transitions = pipeline.Transitions ?? [];

            context = new EtlExecutionContext(pipeline, _activityLibrary, _serviceProvider, _logger, cancellationToken);

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    context.Parameters[param.Key] = param.Value;
                }
            }

            var startActivities = activities.Where(a => a.IsStart).ToList();

            if (startActivities.Count == 0)
            {
                throw new InvalidOperationException("Pipeline has no start activity.");
            }

            var activitiesById = new Dictionary<string, EtlActivityRecord>(StringComparer.Ordinal);
            foreach (var activity in activities)
            {
                if (!string.IsNullOrEmpty(activity.ActivityId))
                {
                    activitiesById[activity.ActivityId] = activity;
                }
            }

            var transitionsBySource = new Dictionary<string, List<EtlTransition>>(StringComparer.Ordinal);
            foreach (var transition in transitions)
            {
                if (string.IsNullOrEmpty(transition.SourceActivityId))
                {
                    continue;
                }

                if (!transitionsBySource.TryGetValue(transition.SourceActivityId, out var sourceTransitions))
                {
                    sourceTransitions = [];
                    transitionsBySource[transition.SourceActivityId] = sourceTransitions;
                }

                sourceTransitions.Add(transition);
            }

            var scheduled = new Stack<ScheduledActivity>();

            foreach (var start in startActivities)
            {
                scheduled.Push(ScheduledActivity.Create(start, context.DataStream));
            }

            while (scheduled.Count > 0 && !cancellationToken.IsCancellationRequested)
            {
                var scheduledActivity = scheduled.Pop();
                var activityRecord = scheduledActivity.Activity;
                context.DataStream = scheduledActivity.DataStream;

                var activity = _activityLibrary.InstantiateActivity(activityRecord.Name);

                if (activity == null)
                {
                    log.Errors.Add($"Activity '{activityRecord.Name}' not found.");
                    log.ErrorCount++;
                    continue;
                }

                activity.Properties = activityRecord.Properties?.DeepClone() as JsonObject ?? [];

                cancellationToken.ThrowIfCancellationRequested();

                var result = await activity.ExecuteAsync(context);

                if (!result.IsSuccess)
                {
                    log.Errors.Add($"Activity '{activityRecord.Name}': {result.ErrorMessage}");
                    log.ErrorCount++;
                    continue;
                }

                if (string.IsNullOrEmpty(activityRecord.ActivityId) ||
                    !transitionsBySource.TryGetValue(activityRecord.ActivityId, out var sourceTransitions))
                {
                    continue;
                }

                foreach (var outcome in result.Outcomes)
                {
                    foreach (var transition in sourceTransitions)
                    {
                        if (!string.Equals(transition.SourceOutcomeName, outcome, StringComparison.Ordinal))
                        {
                            continue;
                        }

                        if (string.IsNullOrEmpty(transition.DestinationActivityId) ||
                            !activitiesById.TryGetValue(transition.DestinationActivityId, out var next))
                        {
                            continue;
                        }

                        if (!scheduledActivity.TryCreateNext(next, context.DataStream, out var nextActivity))
                        {
                            log.Errors.Add($"Cycle detected in ETL pipeline at activity '{next.Name}' ({next.ActivityId}).");
                            log.ErrorCount++;
                            continue;
                        }

                        scheduled.Push(nextActivity);
                    }
                }
            }

            log.Status = cancellationToken.IsCancellationRequested ? "Cancelled" :
                         log.ErrorCount > 0 ? "Failed" : "Success";

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation(
                    "ETL pipeline '{PipelineName}' completed with status {Status}.",
                    pipeline.Name, log.Status);
            }
        }
        catch (OperationCanceledException)
        {
            log.Status = "Cancelled";
            if (_logger.IsEnabled(LogLevel.Warning))
            {
                _logger.LogWarning("ETL pipeline '{PipelineName}' was cancelled.", pipeline.Name);
            }
        }
        catch (Exception ex)
        {
            log.Status = "Failed";
            log.Errors.Add(ex.Message);
            log.ErrorCount++;
            if (_logger.IsEnabled(LogLevel.Error))
            {
                _logger.LogError(ex, "ETL pipeline '{PipelineName}' failed with an unhandled error.", pipeline.Name);
            }
        }
        finally
        {
            if (context != null)
            {
                log.RecordsProcessed = context.RecordsProcessed;
                log.RecordsLoaded = context.RecordsLoaded;
            }

            log.CompletedUtc = DateTime.UtcNow;
        }

        return log;
    }

    private sealed class ScheduledActivity
    {
        private ScheduledActivity(
            EtlActivityRecord activity,
            IAsyncEnumerable<JsonObject> dataStream,
            HashSet<string> visitedActivityIds)
        {
            Activity = activity;
            DataStream = dataStream;
            VisitedActivityIds = visitedActivityIds;
        }

        public EtlActivityRecord Activity { get; }

        public IAsyncEnumerable<JsonObject> DataStream { get; }

        private HashSet<string> VisitedActivityIds { get; }

        public static ScheduledActivity Create(EtlActivityRecord activity, IAsyncEnumerable<JsonObject> dataStream)
        {
            var visitedActivityIds = new HashSet<string>(StringComparer.Ordinal);

            if (!string.IsNullOrEmpty(activity.ActivityId))
            {
                visitedActivityIds.Add(activity.ActivityId);
            }

            return new ScheduledActivity(activity, dataStream, visitedActivityIds);
        }

        public bool TryCreateNext(
            EtlActivityRecord activity,
            IAsyncEnumerable<JsonObject> dataStream,
            out ScheduledActivity scheduledActivity)
        {
            if (!string.IsNullOrEmpty(activity.ActivityId) && VisitedActivityIds.Contains(activity.ActivityId))
            {
                scheduledActivity = null;
                return false;
            }

            var visitedActivityIds = new HashSet<string>(VisitedActivityIds, StringComparer.Ordinal);

            if (!string.IsNullOrEmpty(activity.ActivityId))
            {
                visitedActivityIds.Add(activity.ActivityId);
            }

            scheduledActivity = new ScheduledActivity(activity, dataStream, visitedActivityIds);

            return true;
        }
    }
}
