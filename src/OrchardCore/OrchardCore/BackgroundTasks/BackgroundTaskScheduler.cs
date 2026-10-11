using NCrontab;
using OrchardCore.Modules;

namespace OrchardCore.BackgroundTasks;

public sealed class BackgroundTaskScheduler
{
    /// <summary>
    /// The default number of completed runs kept in <see cref="BackgroundTaskState.RecentRuns"/>.
    /// </summary>
    public const int DefaultMaxRecentRuns = 10;

    private readonly IClock _clock;
    private readonly int _maxRecentRuns;
    private readonly Lock _lock = new();
    private readonly List<BackgroundTaskRun> _recentRuns = [];
    private BackgroundTaskTrigger _trigger;

    public BackgroundTaskScheduler(string tenant, string name, DateTime referenceTime, IClock clock)
        : this(tenant, name, referenceTime, clock, DefaultMaxRecentRuns)
    {
    }

    public BackgroundTaskScheduler(string tenant, string name, DateTime referenceTime, IClock clock, int maxRecentRuns)
    {
        Name = name;
        Tenant = tenant;
        ReferenceTime = referenceTime;
        Settings = new BackgroundTaskSettings() { Name = name };
        State = new BackgroundTaskState() { Name = name, Tenant = tenant };
        _clock = clock;
        _maxRecentRuns = Math.Max(0, maxRecentRuns);
    }

    public string Name { get; }
    public string Tenant { get; }
    public DateTime ReferenceTime { get; set; }
    public BackgroundTaskSettings Settings { get; set; }
    public BackgroundTaskState State { get; set; }
    public ITimeZone TimeZone { get; set; }
    public bool Released { get; set; }
    public bool Updated { get; set; }

    public bool CanRun()
    {
        // A run requested on demand doesn't depend on the schedule, nor on whether the task is enabled.
        if (State.Status == BackgroundTaskStatus.Queued && !Released && Updated)
        {
            return true;
        }

        var now = _clock.UtcNow;
        var referenceTime = ReferenceTime;

        if (TimeZone != null)
        {
            now = _clock.ConvertToTimeZone(now, TimeZone).DateTime;
            referenceTime = _clock.ConvertToTimeZone(ReferenceTime, TimeZone).DateTime;
        }

        var nextStartTime = CrontabSchedule.Parse(Settings.Schedule).GetNextOccurrence(referenceTime);
        if (now >= nextStartTime)
        {
            if (Settings.Enable && !Released && Updated)
            {
                return true;
            }

            ReferenceTime = _clock.UtcNow;
        }

        return false;
    }

    /// <summary>
    /// Marks the task as running, before it starts its work.
    /// </summary>
    public void Run()
    {
        lock (_lock)
        {
            _trigger = State.Status == BackgroundTaskStatus.Queued
                ? BackgroundTaskTrigger.Manual
                : BackgroundTaskTrigger.Schedule;

            State.LastStartTime = ReferenceTime = _clock.UtcNow;
            State.Status = BackgroundTaskStatus.Running;
            State.QueuedTime = null;
        }
    }

    /// <summary>
    /// Records the outcome of the current run, after the task completed its work.
    /// </summary>
    /// <param name="result">The outcome of the run.</param>
    /// <param name="exception">The exception thrown by the task, if any.</param>
    public void Complete(BackgroundTaskRunResult result, Exception exception = null)
    {
        lock (_lock)
        {
            var run = new BackgroundTaskRun
            {
                StartTime = State.LastStartTime,
                EndTime = _clock.UtcNow,
                Trigger = _trigger,
                Result = result,
                ErrorMessage = exception?.Message,
                ErrorDetails = exception?.ToString(),
            };

            State.LastRun = run;
            State.RunCount++;

            if (result == BackgroundTaskRunResult.Failed)
            {
                State.FailureCount++;
            }

            if (_maxRecentRuns > 0)
            {
                _recentRuns.Insert(0, run);
                if (_recentRuns.Count > _maxRecentRuns)
                {
                    _recentRuns.RemoveRange(_maxRecentRuns, _recentRuns.Count - _maxRecentRuns);
                }

                State.RecentRuns = _recentRuns.ToArray();
            }

            if (State.Status == BackgroundTaskStatus.Running)
            {
                State.Status = BackgroundTaskStatus.Idle;
            }
        }
    }

    /// <summary>
    /// Requests the task to run as soon as possible, unless it is already running or queued.
    /// </summary>
    public BackgroundTaskRunRequestResult RequestRun()
    {
        lock (_lock)
        {
            switch (State.Status)
            {
                case BackgroundTaskStatus.Running:
                    return BackgroundTaskRunRequestResult.AlreadyRunning;

                case BackgroundTaskStatus.Queued:
                    return BackgroundTaskRunRequestResult.AlreadyQueued;
            }

            State.Status = BackgroundTaskStatus.Queued;
            State.QueuedTime = _clock.UtcNow;

            return BackgroundTaskRunRequestResult.Queued;
        }
    }

    /// <summary>
    /// Gets a snapshot of the execution state of the task, that is not updated by the next runs.
    /// </summary>
    public BackgroundTaskState GetState()
    {
        lock (_lock)
        {
            return new BackgroundTaskState
            {
                Name = Name,
                Tenant = Tenant,
                LastStartTime = State.LastStartTime,
                Status = State.Status,
                QueuedTime = State.QueuedTime,
                NextStartTime = GetNextStartTime(),
                LastRun = State.LastRun,
                RunCount = State.RunCount,
                FailureCount = State.FailureCount,
                RecentRuns = State.RecentRuns,
            };
        }
    }

    /// <summary>
    /// Gets the UTC date and time of the next scheduled occurrence, or <see langword="null"/> if the task is disabled
    /// or if its schedule is not a valid cron expression.
    /// </summary>
    public DateTime? GetNextStartTime()
    {
        var settings = Settings;
        if (!settings.Enable)
        {
            return null;
        }

        var schedule = CrontabSchedule.TryParse(settings.Schedule);
        if (schedule is null)
        {
            return null;
        }

        var timeZone = TimeZone;
        if (timeZone is null)
        {
            return DateTime.SpecifyKind(schedule.GetNextOccurrence(ReferenceTime), DateTimeKind.Utc);
        }

        // The schedule applies to the local time of the tenant, as when checking if the task can run.
        var localReferenceTime = _clock.ConvertToTimeZone(ReferenceTime, timeZone).DateTime;
        var localNextStartTime = schedule.GetNextOccurrence(localReferenceTime);

        // The offset depends on the instant, so it is first approximated with the offset at the local time read as UTC.
        var offset = _clock.ConvertToTimeZone(DateTime.SpecifyKind(localNextStartTime, DateTimeKind.Utc), timeZone).Offset;
        var nextStartTime = DateTime.SpecifyKind(localNextStartTime - offset, DateTimeKind.Utc);

        offset = _clock.ConvertToTimeZone(nextStartTime, timeZone).Offset;

        return DateTime.SpecifyKind(localNextStartTime - offset, DateTimeKind.Utc);
    }
}
