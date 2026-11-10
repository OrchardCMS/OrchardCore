using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.Localization;
using OrchardCore.Workflows;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Forms.Activities;

public class HttpRedirectToFormLocationTask : TaskActivity<HttpRedirectToFormLocationTask>, IActivityDescription
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IStringLocalizerFactory _localizerFactory;
    protected readonly IStringLocalizer S;

    public HttpRedirectToFormLocationTask(
        IHttpContextAccessor httpContextAccessor,
        IStringLocalizerFactory localizerFactory,
        IStringLocalizer<HttpRedirectToFormLocationTask> stringLocalizer
    )
    {
        _httpContextAccessor = httpContextAccessor;
        _localizerFactory = localizerFactory;
        S = stringLocalizer;
    }

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<HttpRedirectToFormLocationTask>("Http Redirect To Form Location Task");

    public static LocalizationSource CategoryName => ActivityCategoryLocalizationSources.Http;

    public override LocalizedString DisplayText => _localizerFactory.Localize(DisplayName);

    public override LocalizedString Category => _localizerFactory.Localize(CategoryName);

    public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        => Outcome(S["Done"], S["Failed"]);

    public string FormLocationKey
    {
        get => GetProperty(() => string.Empty);
        set => SetProperty(value ?? string.Empty);
    }

    public override Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        if (workflowContext.Output.TryGetValue(WorkflowConstants.HttpFormLocationOutputKeyName, out var obj)
            && obj is Dictionary<string, string> formLocations)
        {
            if (formLocations.TryGetValue(FormLocationKey, out var path))
            {
                _httpContextAccessor.HttpContext.Items[WorkflowConstants.FormOriginatedLocationItemsKey] = path;

                return Task.FromResult(Outcome("Done"));
            }
        }

        return Task.FromResult(Outcome("Failed"));
    }
}
