using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;
using YesSql;

namespace OrchardCore.Workflows.Activities;

public class CommitTransactionTask : TaskActivity<CommitTransactionTask>, IActivityDescription
{
    private readonly ISession _session;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly IStringLocalizerFactory _localizerFactory;
    protected readonly IStringLocalizer S;

    public CommitTransactionTask(
        ISession session,
        IUpdateModelAccessor updateModelAccessor,
        IStringLocalizerFactory localizerFactory,
        IStringLocalizer<CommitTransactionTask> localizer)
    {
        _session = session;
        _updateModelAccessor = updateModelAccessor;
        _localizerFactory = localizerFactory;
        S = localizer;
    }

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<CommitTransactionTask>("Commit Transaction Task");

    public static LocalizationSource CategoryName => ActivityCategoryLocalizationSources.Session;

    public override LocalizedString DisplayText => _localizerFactory.Localize(DisplayName);

    public override LocalizedString Category => _localizerFactory.Localize(CategoryName);

    public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        => Outcome(S["Done"], S["Valid"], S["Invalid"]);

    public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        if (!_updateModelAccessor.ModelUpdater.ModelState.IsValid)
        {
            return Outcome("Done", "Invalid");
        }

        await _session.SaveChangesAsync(workflowContext.CancellationToken);

        return Outcome("Done", "Valid");
    }
}
