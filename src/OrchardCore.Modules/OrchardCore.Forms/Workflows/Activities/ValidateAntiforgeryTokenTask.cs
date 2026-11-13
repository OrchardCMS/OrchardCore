using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Forms.Workflows.Activities;

public class ValidateAntiforgeryTokenTask : TaskActivity<ValidateAntiforgeryTokenTask>, IActivityDescription
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAntiforgery _antiforgery;
    private readonly IStringLocalizerFactory _localizerFactory;
    protected readonly IStringLocalizer S;

    public ValidateAntiforgeryTokenTask(
        IHttpContextAccessor httpContextAccessor,
        IAntiforgery antiforgery,
        IStringLocalizerFactory localizerFactory,
        IStringLocalizer<ValidateAntiforgeryTokenTask> localizer
    )
    {
        _httpContextAccessor = httpContextAccessor;
        _antiforgery = antiforgery;
        _localizerFactory = localizerFactory;
        S = localizer;
    }

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<ValidateAntiforgeryTokenTask>("Validate Antiforgery Token Task");

    public static LocalizationSource CategoryName => ActivityCategoryLocalizationSources.Validation;

    public override LocalizedString DisplayText => _localizerFactory.Localize(DisplayName);

    public override LocalizedString Category => _localizerFactory.Localize(CategoryName);

    public override bool HasEditor => false;

    public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        => Outcome(S["Done"], S["Valid"], S["Invalid"]);

    public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        if (await _antiforgery.IsRequestValidAsync(_httpContextAccessor.HttpContext))
        {
            return Outcome("Done", "Valid");
        }
        else
        {
            return Outcome("Done", "Invalid");
        }
    }
}
