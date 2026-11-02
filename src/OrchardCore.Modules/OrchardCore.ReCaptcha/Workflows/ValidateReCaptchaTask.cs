using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Localization;
using OrchardCore.ReCaptcha.Services;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace OrchardCore.ReCaptcha.Workflows;

public class ValidateReCaptchaTask : TaskActivity<ValidateReCaptchaTask>, IActivityDescription
{
    private readonly ReCaptchaService _reCaptchaService;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly IStringLocalizerFactory _localizerFactory;
    protected readonly IStringLocalizer S;

    public ValidateReCaptchaTask(
        ReCaptchaService reCaptchaService,
        IUpdateModelAccessor updateModelAccessor,
        IStringLocalizerFactory localizerFactory,
        IStringLocalizer<ValidateReCaptchaTask> localizer
    )
    {
        _reCaptchaService = reCaptchaService;
        _updateModelAccessor = updateModelAccessor;
        _localizerFactory = localizerFactory;
        S = localizer;
    }

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<ValidateReCaptchaTask>("Validate ReCaptcha Task");

    public static LocalizationSource CategoryName => ActivityCategoryLocalizationSources.Validation;

    public override LocalizedString DisplayText => _localizerFactory.Localize(DisplayName);

    public override LocalizedString Category => _localizerFactory.Localize(CategoryName);

    public override bool HasEditor => false;

    public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        => Outcome(S["Done"], S["Valid"], S["Invalid"]);

    public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        var outcome = "Valid";

        await _reCaptchaService.ValidateCaptchaAsync((key, error) =>
        {
            var updater = _updateModelAccessor.ModelUpdater;
            outcome = "Invalid";

            updater?.ModelState.TryAddModelError(Constants.ReCaptchaServerResponseHeaderName, S["Captcha validation failed. Try again."]);
        });

        return Outcome("Done", outcome);
    }
}
