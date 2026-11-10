using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Forms.Workflows.Activities;

// TODO: Add the ability to configure various types of validators.
public class ValidateFormFieldTask : TaskActivity<ValidateFormFieldTask>, IActivityDescription
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly IStringLocalizerFactory _localizerFactory;
    protected readonly IStringLocalizer S;

    public ValidateFormFieldTask(
        IHttpContextAccessor httpContextAccessor,
        IUpdateModelAccessor updateModelAccessor,
        IStringLocalizerFactory localizerFactory,
        IStringLocalizer<ValidateFormFieldTask> localizer
    )
    {
        _httpContextAccessor = httpContextAccessor;
        _updateModelAccessor = updateModelAccessor;
        _localizerFactory = localizerFactory;
        S = localizer;
    }

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<ValidateFormFieldTask>("Validate Form Field Task");

    public static LocalizationSource CategoryName => ActivityCategoryLocalizationSources.Validation;

    public override LocalizedString DisplayText => _localizerFactory.Localize(DisplayName);

    public override LocalizedString Category => _localizerFactory.Localize(CategoryName);

    public string FieldName
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
        => Outcome(S["Done"], S["Valid"], S["Invalid"]);

    public override ActivityExecutionResult Execute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        var form = _httpContextAccessor.HttpContext.Request.Form;
        var fieldValue = form[FieldName];
        var isValid = !string.IsNullOrWhiteSpace(fieldValue);
        var outcome = isValid ? "Valid" : "Invalid";

        if (!isValid)
        {
            var updater = _updateModelAccessor.ModelUpdater;

            updater?.ModelState.TryAddModelError(FieldName, ErrorMessage);
        }

        return Outcome("Done", outcome);
    }
}
