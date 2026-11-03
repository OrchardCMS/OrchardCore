using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Forms.Workflows.Activities;

public class ValidateFormTask : TaskActivity<ValidateFormTask>, IActivityDescription
{
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly IStringLocalizerFactory _localizerFactory;
    protected readonly IStringLocalizer S;

    public ValidateFormTask(
        IUpdateModelAccessor updateModelAccessor,
        IStringLocalizerFactory localizerFactory,
        IStringLocalizer<ValidateFormTask> localizer
    )
    {
        _updateModelAccessor = updateModelAccessor;
        _localizerFactory = localizerFactory;
        S = localizer;
    }

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<ValidateFormTask>("Validate Form Task");

    public static LocalizationSource CategoryName => ActivityCategoryLocalizationSources.Validation;

    public override LocalizedString DisplayText => _localizerFactory.Localize(DisplayName);

    public override LocalizedString Category => _localizerFactory.Localize(CategoryName);

    public override bool HasEditor => false;

    public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        => Outcome(S["Valid"], S["Invalid"]);

    public override ActivityExecutionResult Execute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        var updater = _updateModelAccessor.ModelUpdater
            ?? throw new InvalidOperationException("Cannot add model validation errors when there's no Updater present.");

        var isValid = updater.ModelState.ErrorCount == 0;
        var outcome = isValid ? "Valid" : "Invalid";

        return Outcome(outcome);
    }
}
