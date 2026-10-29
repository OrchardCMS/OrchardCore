using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Activities;

/// <summary>
/// A replacement for when a referenced activity could not be found. This is a system-level activity that is not registered with WorkflowOptions.
/// </summary>
public class MissingActivity : Activity, IActivityDescription
{
    private readonly ILogger _logger;
    private readonly IStringLocalizerFactory _localizerFactory;
    protected readonly IStringLocalizer S;

    public MissingActivity(IStringLocalizerFactory localizerFactory, IStringLocalizer<MissingActivity> localizer, ILogger<MissingActivity> logger, ActivityRecord missingActivityRecord)
    {
        _localizerFactory = localizerFactory;
        S = localizer;
        _logger = logger;
        MissingActivityRecord = missingActivityRecord;
    }

    public override string Name => nameof(MissingActivity);

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<MissingActivity>("Missing Activity");

    public static LocalizationSource CategoryName => ActivityCategoryLocalizationSources.Exceptions;

    public override LocalizedString DisplayText => _localizerFactory.Localize(DisplayName);

    public override LocalizedString Category => _localizerFactory.Localize(CategoryName);

    public ActivityRecord MissingActivityRecord { get; }

    public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        yield break;
    }

    public override ActivityExecutionResult Execute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        _logger.LogWarning("Activity '{ActivityName}' is no longer available. This can happen if the feature providing the activity is no longer enabled. Either enable the feature, or remove this activity from workflow definition with ID {WorkflowTypeId}", MissingActivityRecord.Name, workflowContext.WorkflowType.Id);
        return Noop();
    }
}
