using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using NCrontab;
using OrchardCore.Localization;
using OrchardCore.Modules;
using OrchardCore.Settings;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Timers;

public class TimerEvent : EventActivity, IActivityProvidedValues, IActivityDescription
{
    public static string EventName => nameof(TimerEvent);

    private readonly IClock _clock;
    private readonly ISiteService _siteService;
    private readonly ILogger _logger;
    private readonly IStringLocalizerFactory _localizerFactory;
    protected readonly IStringLocalizer S;

    public TimerEvent(IClock clock, ISiteService siteService, ILogger<TimerEvent> logger, IStringLocalizerFactory localizerFactory, IStringLocalizer<TimerEvent> localizer)
    {
        _clock = clock;
        _siteService = siteService;
        _logger = logger;
        _localizerFactory = localizerFactory;
        S = localizer;
    }

    public override string Name => EventName;

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<TimerEvent>("Timer Event");

    public static LocalizationSource CategoryName => ActivityCategoryLocalizationSources.Background;

    public override LocalizedString DisplayText => _localizerFactory.Localize(DisplayName);

    public override LocalizedString Category => _localizerFactory.Localize(CategoryName);

    public string CronExpression
    {
        get => GetProperty(() => "*/5 * * * *");
        set => SetProperty(value);
    }

    public bool UseLocalTime
    {
        get => GetProperty(() => false);
        set => SetProperty(value);
    }

    private DateTime? StartedUtc
    {
        get => GetProperty<DateTime?>();
        set => SetProperty(value);
    }

    private static readonly ActivityProvidedValue[] s_providedValues = [ActivityProvidedValue.LastResult("string", LocalizationSource.Create<TimerEvent>("The text TimerEvent."))];

    public IEnumerable<ActivityProvidedValue> GetProvidedValues()
        => s_providedValues;

    public override ActivityExecutionResult Execute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        // The timer starts when the workflow starts waiting on it, not when the background task first looks at it, so the
        // first run of the task after the timer is due resumes the workflow, and a run before that has nothing to save.
        StartedUtc ??= _clock.UtcNow;

        return base.Execute(workflowContext, activityContext);
    }

    public override async Task<bool> CanExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        return StartedUtc == null || await IsExpiredAsync();
    }

    public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        => Outcome(S["Done"]);

    public override async Task<ActivityExecutionResult> ResumeAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        if (await IsExpiredAsync())
        {
            workflowContext.LastResult = "TimerEvent";

            return Outcome("Done");
        }

        return Halt();
    }

    private async Task<bool> IsExpiredAsync()
    {
        StartedUtc ??= _clock.UtcNow;
        var schedule = CrontabSchedule.Parse(CronExpression);

        ITimeZone timeZone = null;

        if (UseLocalTime && _siteService is not null)
        {
            try
            {
                timeZone = _clock.GetTimeZone((await _siteService.GetSiteSettingsAsync()).TimeZoneId);
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                _logger.LogError(ex, "Error while getting the time zone from the site settings.");
            }
        }

        var now = _clock.UtcNow;
        var baseTime = StartedUtc.Value;

        if (timeZone is not null)
        {
            now = _clock.ConvertToTimeZone(now, timeZone).DateTime;
            baseTime = _clock.ConvertToTimeZone(baseTime, timeZone).DateTime;
        }

        var nextOccurrence = schedule.GetNextOccurrence(baseTime);

        return now >= nextOccurrence;
    }
}
