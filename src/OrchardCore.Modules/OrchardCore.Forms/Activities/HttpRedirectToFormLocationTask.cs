using System.Collections.Generic;
using System.Threading.Tasks;
using Fluid;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Localization;
using OrchardCore.Forms.Drivers;
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

    public WorkflowExpression<string> FormLocationKey
    {
        get => GetProperty(() => new WorkflowExpression<string>());
        set => SetProperty(value);
    }

    public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        var location = await _workflowExpressionEvaluator.EvaluateAsync(FormLocationKey, workflowContext, NullEncoder.Default);
        if (string.IsNullOrEmpty(location))
        {
            location = FormPartDisplayDriver.DefaultFormLocationInputName;
        }

        if (_httpContextAccessor.HttpContext.Request.HasFormContentType
            && _httpContextAccessor.HttpContext.Request.Form.TryGetValue(location, out var value)
            && !string.IsNullOrWhiteSpace(value))
        {
            _httpContextAccessor.HttpContext.Items[WorkflowConstants.FormOriginatedLocationItemsKey] = GetLocationUrl(value.ToString());

            return Outcomes("Done");
        }

        return Outcomes("Failed");
    }

    private static string GetLocationUrl(string value)
    {
        if (value.StartsWith('/'))
        {
            return "~" + value;
        }

        return value;
    }
}
