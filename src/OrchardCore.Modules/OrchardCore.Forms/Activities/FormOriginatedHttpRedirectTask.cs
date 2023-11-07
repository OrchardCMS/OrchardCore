using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Localization;
using OrchardCore.Forms.Models;
using OrchardCore.Workflows;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Forms.Activities;

public class FormOriginatedHttpRedirectTask : TaskActivity
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    protected readonly IStringLocalizer S;
    private readonly IActionContextAccessor _actionContextAccessor;

    public FormOriginatedHttpRedirectTask(
        IStringLocalizer<FormOriginatedHttpRedirectTask> localizer,
        IActionContextAccessor actionContextAccessor,
        IHttpContextAccessor httpContextAccessor
    )
    {
        S = localizer;
        _actionContextAccessor = actionContextAccessor;
        _httpContextAccessor = httpContextAccessor;
    }

    public override string Name => nameof(FormOriginatedHttpRedirectTask);

    public override LocalizedString DisplayText => S["Form Originated Http Redirect Task"];

    public override LocalizedString Category => S["HTTP"];

    public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        return Outcomes(S["Done"], S["Failed"]);
    }

    public override Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        if (_httpContextAccessor.HttpContext.Request.HasFormContentType
            && _httpContextAccessor.HttpContext.Request.Form.TryGetValue(FormPart.RequestOriginatedFromInputName, out var value)
            && !string.IsNullOrWhiteSpace(value))
        {
            _httpContextAccessor.HttpContext.Items[WorkflowConstants.FormOriginatedLocationItemsKey] = GetLocationUrl(value.ToString());

            return Task.FromResult(Outcomes("Done"));
        }

        return Task.FromResult(Outcomes("Failed"));
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
