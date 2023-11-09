using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Forms.Activities;

public class HttpRedirectToFormLocationTask : TaskActivity<HttpRedirectToFormLocationTask>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    protected readonly IStringLocalizer S;

    public HttpRedirectToFormLocationTask(
        IHttpContextAccessor httpContextAccessor,
        IStringLocalizer<HttpRedirectToFormLocationTask> stringLocalizer
    )
    {
        _httpContextAccessor = httpContextAccessor;
        S = stringLocalizer;
    }

    public override LocalizedString DisplayText => S["Http Redirect To Form Location Task"];

    public override LocalizedString Category => S["HTTP"];

    public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        => Outcomes(S["Done"], S["Failed"]);

    public string FormLocationKey
    {
        get => GetProperty<string>();
        set => SetProperty(value);
    }

    public override Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        if (workflowContext.Output.TryGetValue(WorkflowConstants.HttpFormLocationOutputKeyName, out var obj)
            && obj is Dictionary<string, string> formLocations)
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
