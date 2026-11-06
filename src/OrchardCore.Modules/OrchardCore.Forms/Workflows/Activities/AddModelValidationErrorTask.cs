using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Forms.Workflows.Activities;

public class AddModelValidationErrorTask : TaskActivity<AddModelValidationErrorTask>, IActivityDescription
{
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly IStringLocalizerFactory _localizerFactory;
    protected readonly IStringLocalizer S;

    public AddModelValidationErrorTask(
        IUpdateModelAccessor updateModelAccessor,
        IStringLocalizerFactory localizerFactory,
        IStringLocalizer<AddModelValidationErrorTask> localizer
    )
    {
        _updateModelAccessor = updateModelAccessor;
        _localizerFactory = localizerFactory;
        S = localizer;
    }

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<AddModelValidationErrorTask>("Add Model Validation Error Task");

    public static LocalizationSource CategoryName => ActivityCategoryLocalizationSources.Validation;

    public override LocalizedString DisplayText => _localizerFactory.Localize(DisplayName);

    public override LocalizedString Category => _localizerFactory.Localize(CategoryName);

    public string Key
    {
        get => GetProperty<string>();
        set => SetProperty(value);
    }

    public string ErrorMessage
    {
        get => GetProperty<string>();
        set => SetProperty(value);
    }

    public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        => Outcome(S["Done"]);

    public override ActivityExecutionResult Execute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        var updater = _updateModelAccessor.ModelUpdater
            ?? throw new InvalidOperationException("Cannot add model validation errors when there's no Updater present.");

        updater.ModelState.AddModelError(Key, ErrorMessage);

        return Outcome("Done");
    }
}
