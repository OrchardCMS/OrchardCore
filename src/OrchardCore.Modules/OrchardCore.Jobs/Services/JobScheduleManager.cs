using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCrontab;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Jobs.Indexes;
using OrchardCore.Jobs.Models;
using OrchardCore.Modules;
using OrchardCore.Settings;
using YesSql;
using IIdGenerator = OrchardCore.Entities.IIdGenerator;
using ISession = YesSql.ISession;

namespace OrchardCore.Jobs.Services;

/// <summary>
/// Manages the schedules, and queues the runs of the ones that are due.
/// </summary>
public sealed class JobScheduleManager : IJobScheduleManager
{
    private readonly ISession _session;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly ISiteService _siteService;
    private readonly JobOptions _jobOptions;
    private readonly ILogger _logger;
    private ITimeZone _timeZone;

    public JobScheduleManager(
        ISession session,
        IIdGenerator idGenerator,
        IClock clock,
        ISiteService siteService,
        IOptions<JobOptions> jobOptions,
        ILogger<JobScheduleManager> logger)
    {
        _session = session;
        _idGenerator = idGenerator;
        _clock = clock;
        _siteService = siteService;
        _jobOptions = jobOptions.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<JobSchedule>> ListAsync()
    {
        var schedules = await EnsureDeclaredAsync();

        return schedules
            .OrderBy(schedule => schedule.JobName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(schedule => schedule.CreatedUtc)
            .ToList();
    }

    /// <inheritdoc />
    public async Task<JobSchedule> GetAsync(string scheduleId)
    {
        if (string.IsNullOrEmpty(scheduleId))
        {
            return null;
        }

        var id = JobRunIndexProvider.Truncate(scheduleId, JobRunIndex.MaxKeyLength);

        return await _session.Query<JobSchedule, JobScheduleIndex>(index => index.ScheduleId == id, collection: JobRun.Collection).FirstOrDefaultAsync();
    }

    /// <inheritdoc />
    public async Task SaveAsync(JobSchedule schedule)
    {
        ArgumentNullException.ThrowIfNull(schedule);

        if (_jobOptions.Find(schedule.JobName) is null)
        {
            throw new ArgumentException($"No job is registered with the name '{schedule.JobName}'.", nameof(schedule));
        }

        if (!IsValidCron(schedule.Cron))
        {
            throw new ArgumentException($"'{schedule.Cron}' is not a valid cron expression.", nameof(schedule));
        }

        var now = _clock.UtcNow;

        if (string.IsNullOrEmpty(schedule.ScheduleId))
        {
            schedule.ScheduleId = _idGenerator.GenerateUniqueId();
            schedule.CreatedUtc = now;
        }
        else
        {
            schedule.ModifiedUtc = now;
        }

        schedule.NextOccurrenceUtc = await GetNextOccurrenceAsync(schedule.Cron, now);

        await _session.SaveAsync(schedule, collection: JobRun.Collection);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(string scheduleId)
    {
        var schedule = await GetAsync(scheduleId);

        // The schedule a registered job declares would be created again.
        if (schedule is null || (schedule.IsDeclared && _jobOptions.Find(schedule.JobName)?.Schedule is not null))
        {
            return false;
        }

        _session.Delete(schedule, collection: JobRun.Collection);

        return true;
    }

    /// <inheritdoc />
    public bool IsValidCron(string cron)
    {
        return !string.IsNullOrWhiteSpace(cron) && CrontabSchedule.TryParse(cron) is not null;
    }

    /// <summary>
    /// Gets the occurrence of a cron expression that follows a date, in UTC. The expression applies to the local
    /// time of the site.
    /// </summary>
    /// <param name="cron">The cron expression.</param>
    /// <param name="afterUtc">The date the occurrence follows, in UTC.</param>
    /// <returns>The occurrence, or <see langword="null"/> when the expression is not valid or never occurs.</returns>
    public async Task<DateTime?> GetNextOccurrenceAsync(string cron, DateTime afterUtc)
    {
        var schedule = string.IsNullOrWhiteSpace(cron) ? null : CrontabSchedule.TryParse(cron);

        if (schedule is null)
        {
            return null;
        }

        _timeZone ??= _clock.GetTimeZone((await _siteService.GetSiteSettingsAsync()).TimeZoneId);

        var localAfter = _clock.ConvertToTimeZone(afterUtc, _timeZone).DateTime;
        var localNext = schedule.GetNextOccurrence(localAfter);

        // No occurrence was found before the maximum date, which can't be converted to another time zone.
        if (localNext == DateTime.MaxValue)
        {
            return null;
        }

        // The offset depends on the instant, so it is first approximated with the offset at the local time read as UTC.
        var offset = _clock.ConvertToTimeZone(DateTime.SpecifyKind(localNext, DateTimeKind.Utc), _timeZone).Offset;
        var next = DateTime.SpecifyKind(localNext - offset, DateTimeKind.Utc);

        offset = _clock.ConvertToTimeZone(next, _timeZone).Offset;
        next = DateTime.SpecifyKind(localNext - offset, DateTimeKind.Utc);

        // A time that occurs twice when the clocks go back must not be found again.
        return next > afterUtc ? next : afterUtc.AddMinutes(1);
    }

    /// <summary>
    /// Creates the schedules the registered jobs declare that don't exist yet, and deletes the ones that were
    /// created twice by servers starting at the same time.
    /// </summary>
    /// <returns>The schedules.</returns>
    public async Task<List<JobSchedule>> EnsureDeclaredAsync()
    {
        var schedules = (await _session.Query<JobSchedule, JobScheduleIndex>(collection: JobRun.Collection).ListAsync()).ToList();

        foreach (var duplicate in schedules.GroupBy(schedule => schedule.ScheduleId, StringComparer.OrdinalIgnoreCase).SelectMany(group => group.OrderBy(schedule => schedule.Id).Skip(1)).ToList())
        {
            _session.Delete(duplicate, collection: JobRun.Collection);
            schedules.Remove(duplicate);
        }

        var now = _clock.UtcNow;

        foreach (var descriptor in _jobOptions.Jobs.Values)
        {
            if (string.IsNullOrEmpty(descriptor.Schedule) || schedules.Any(schedule => string.Equals(schedule.ScheduleId, descriptor.Name, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            var schedule = new JobSchedule
            {
                ScheduleId = descriptor.Name,
                JobName = descriptor.Name,
                Cron = descriptor.Schedule,
                IsEnabled = descriptor.ScheduleEnabled,
                IsDeclared = true,
                CreatedUtc = now,
                NextOccurrenceUtc = await GetNextOccurrenceAsync(descriptor.Schedule, now),
            };

            if (schedule.NextOccurrenceUtc is null)
            {
                _logger.LogError("The schedule '{Schedule}' the job '{JobName}' declares is not a valid cron expression.", descriptor.Schedule, descriptor.Name);
            }

            await _session.SaveAsync(schedule, collection: JobRun.Collection);
            schedules.Add(schedule);
        }

        return schedules;
    }

    /// <summary>
    /// Queues a run for each schedule that is due. Whatever the number of servers doing it at the same time, an
    /// occurrence creates a single run: the run is saved together with the next occurrence of its schedule, which
    /// only one server can change.
    /// </summary>
    /// <returns>A task that completes when the due schedules were processed.</returns>
    public async Task QueueDueAsync()
    {
        var now = _clock.UtcNow;

        var due = await _session
            .QueryIndex<JobScheduleIndex>(index => index.IsEnabled && index.NextOccurrenceUtc != null && index.NextOccurrenceUtc <= now, collection: JobRun.Collection)
            .ListAsync();

        foreach (var entry in due)
        {
            var scheduleId = entry.ScheduleId;

            try
            {
                await ShellScope.UsingChildScopeAsync(scope => scope.ServiceProvider.GetRequiredService<JobScheduleManager>().QueueOccurrenceAsync(scheduleId, now));
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                _logger.LogError(ex, "The run of the job schedule '{ScheduleId}' couldn't be queued.", scheduleId);
            }
        }
    }

    private async Task QueueOccurrenceAsync(string scheduleId, DateTime now)
    {
        var schedule = await GetAsync(scheduleId);

        if (schedule is null || !schedule.IsEnabled || schedule.NextOccurrenceUtc is null || schedule.NextOccurrenceUtc > now)
        {
            return;
        }

        // The occurrences missed while no server was running result in a single run.
        schedule.LastOccurrenceUtc = schedule.NextOccurrenceUtc;
        schedule.NextOccurrenceUtc = await GetNextOccurrenceAsync(schedule.Cron, now);

        if (_jobOptions.Find(schedule.JobName) is not null)
        {
            var jobManager = ShellScope.Services.GetRequiredService<IJobManager>();

            // While the run of an earlier occurrence didn't end, no other run is queued.
            var run = await jobManager.EnqueueAsync(schedule.JobName, schedule.Parameters, request =>
            {
                request.Title = schedule.Name;
                request.Trigger = JobTriggers.Schedule;
                request.ScheduleId = schedule.ScheduleId;
                request.DeduplicationKey = $"Schedule:{schedule.ScheduleId}";
            });

            schedule.LastRunId = run.RunId;
        }

        await _session.SaveAsync(schedule, checkConcurrency: true, collection: JobRun.Collection);

        try
        {
            await _session.SaveChangesAsync();
        }
        catch (ConcurrencyException)
        {
            // Another server queued the run of this occurrence: the run of this server was not saved.
        }
    }
}
