using Fluid;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Http.Activities;

public class HttpRedirectTask : TaskActivity<HttpRedirectTask>, IActivityDescription
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
    private readonly IStringLocalizerFactory _localizerFactory;
    protected readonly IStringLocalizer S;

    public HttpRedirectTask(
        IStringLocalizerFactory localizerFactory,
        IStringLocalizer<HttpRedirectTask> localizer,
        IHttpContextAccessor httpContextAccessor,
        IWorkflowExpressionEvaluator expressionEvaluator
    )
    {
        _localizerFactory = localizerFactory;
        S = localizer;
        _httpContextAccessor = httpContextAccessor;
        _expressionEvaluator = expressionEvaluator;
    }

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<HttpRedirectTask>("Http Redirect Task");

    public static LocalizationSource CategoryName => ActivityCategoryLocalizationSources.Http;

    public override LocalizedString DisplayText => _localizerFactory.Localize(DisplayName);

    public override LocalizedString Category => _localizerFactory.Localize(CategoryName);

    public WorkflowExpression<string> Location
    {
        get => GetProperty(() => new WorkflowExpression<string>());
        set => SetProperty(value);
    }

    public bool Permanent
    {
        get => GetProperty(() => false);
        set => SetProperty(value);
    }

    public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        => Outcome(S["Done"]);

    public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        var location = await _expressionEvaluator.EvaluateAsync(Location, workflowContext, NullEncoder.Default);

        _httpContextAccessor.HttpContext.Response.Redirect(location.ToUriComponents(), Permanent);
        _httpContextAccessor.HttpContext.Items[WorkflowHttpResult.Instance] = WorkflowHttpResult.Instance;

        return Outcome("Done");
    }
}
