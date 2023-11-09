using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Forms.Activities;

public class HttpRedirectToFormLocationTask : TaskActivity
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IActionContextAccessor _actionContextAccessor;
    private readonly IWorkflowExpressionEvaluator _workflowExpressionEvaluator;
    protected readonly IStringLocalizer S;

    public HttpRedirectToFormLocationTask(
        IStringLocalizer<HttpRedirectToFormLocationTask> localizer,
        IActionContextAccessor actionContextAccessor,
        IWorkflowExpressionEvaluator workflowExpressionEvaluator,
        IHttpContextAccessor httpContextAccessor
    )
    {
        S = localizer;
        _actionContextAccessor = actionContextAccessor;
        _workflowExpressionEvaluator = workflowExpressionEvaluator;
        _httpContextAccessor = httpContextAccessor;
    }

    public override string Name => nameof(HttpRedirectToFormLocationTask);

    public override LocalizedString DisplayText => S["Http Redirect To Form Location Task"];

    public override LocalizedString Category => S["HTTP"];

    public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        return Outcomes(S["Done"], S["Failed"]);
    }

    public string FormLocationKey
    {
        get => GetProperty<string>();
        set => SetProperty(value);
    }

    public override Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        if (workflowContext.Output.TryGetValue(WorkflowConstants.HttpFormLocationOutputKeyName, out var obj) && obj is Dictionary<string, string> formLocations)
        {
            // if no custom location-key was provided, we use empty string as the default key.
            var location = FormLocationKey ?? string.Empty;

            if (formLocations.TryGetValue(location, out var path))
            {
                _httpContextAccessor.HttpContext.Items[WorkflowConstants.FormOriginatedLocationItemsKey] = path;

                return Task.FromResult(Outcomes("Done"));
            }
        }

        return Task.FromResult(Outcomes("Failed"));
    }
}
