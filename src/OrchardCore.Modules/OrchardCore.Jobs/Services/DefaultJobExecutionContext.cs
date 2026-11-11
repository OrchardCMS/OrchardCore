using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Jobs.Models;
using OrchardCore.Modules;

namespace OrchardCore.Jobs.Services;

/// <summary>
/// The context of a run that executes on this server. It collects what the job reports, which the executor saves
/// with each heartbeat of the run.
/// </summary>
internal sealed class DefaultJobExecutionContext : JobExecutionContext
{
    private readonly IClock _clock;
    private readonly ConcurrentQueue<JobLogEntry> _log = new();
    private readonly Lock _progressLock = new();
    private IServiceProvider _serviceProvider;
    private int _logCount;
    private bool _progressChanged;
    private int? _progress;
    private string _progressMessage;

    public DefaultJobExecutionContext(JobRun run, IClock clock)
    {
        Run = run;
        _clock = clock;
        _logCount = run.Log.Count;
    }

    public override JobRun Run { get; }

    public override IServiceProvider ServiceProvider => _serviceProvider
        ?? throw new InvalidOperationException("The services of a run are only available while its job executes.");

    /// <summary>
    /// Gets the result the job set, if any.
    /// </summary>
    public JsonNode Result { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the job queued child runs.
    /// </summary>
    public bool HasChildren { get; private set; }

    public void SetServiceProvider(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public override void Log(LogLevel level, string message)
    {
        if (string.IsNullOrEmpty(message) || Interlocked.Increment(ref _logCount) > JobRun.MaxLogEntries)
        {
            return;
        }

        _log.Enqueue(new JobLogEntry { Utc = _clock.UtcNow, Level = level, Message = message });
    }

    public override void ReportProgress(int? percent, string message = null)
    {
        lock (_progressLock)
        {
            _progress = percent is null ? null : Math.Clamp(percent.Value, 0, 100);
            _progressMessage = message;
            _progressChanged = true;
        }
    }

    public override void SetResult(object result)
    {
        Result = result switch
        {
            null => null,
            JsonNode node => node.DeepClone(),
            _ => JNode.FromObject(result, JOptions.Default),
        };
    }

    public override Task<JobRun> EnqueueChildAsync(string jobName, object parameters = null, Action<JobRequest> configure = null)
    {
        HasChildren = true;

        return ServiceProvider.GetRequiredService<IJobManager>().EnqueueAsync(jobName, parameters, request =>
        {
            request.Trigger = JobTriggers.Parent;
            request.TriggeredBy = Run.TriggeredBy;
            configure?.Invoke(request);
            request.ParentRunId = Run.RunId;
        });
    }

    /// <summary>
    /// Copies to a run what the job reported since the last call.
    /// </summary>
    public void Apply(JobRun run)
    {
        while (_log.TryDequeue(out var entry))
        {
            if (run.Log.Count < JobRun.MaxLogEntries)
            {
                run.Log.Add(entry);
            }
        }

        lock (_progressLock)
        {
            if (_progressChanged)
            {
                run.Progress = _progress;
                run.ProgressMessage = _progressMessage;
                _progressChanged = false;
            }
        }
    }
}
