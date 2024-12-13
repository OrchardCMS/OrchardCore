using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Http.Activities;

public class HttpRequestEvent : EventActivity
{
    public static string EventName => nameof(HttpRequestEvent);

    private readonly IHttpContextAccessor _httpContextAccessor;
    protected readonly IStringLocalizer S;

    public HttpRequestEvent(
        IStringLocalizer<HttpRequestEvent> localizer,
        IHttpContextAccessor httpContextAccessor
    )
    {
        S = localizer;
        _httpContextAccessor = httpContextAccessor;
    }

    public override string Name => EventName;
    public override LocalizedString DisplayText => S["Http Request Event"];
    public override LocalizedString Category => S["HTTP"];

    public string HttpMethod
    {
        get => GetProperty<string>();
        set => SetProperty(value);
    }

    public string Url
    {
        get => GetProperty<string>();
        set => SetProperty(value);
    }

    public bool ValidateAntiforgeryToken
    {
        get => GetProperty(() => true);
        set => SetProperty(value);
    }

    public int TokenLifeSpan
    {
        get => GetProperty(() => 0);
        set => SetProperty(value);
    }

    public string FormLocationKey
    {
        get => GetProperty(() => string.Empty);
        set => SetProperty(value ?? string.Empty);
    }

    public override bool CanExecute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var httpRequest = httpContext.Request;
        var isMatch = string.Equals(HttpMethod, httpRequest.Method, StringComparison.OrdinalIgnoreCase);

        return isMatch;
    }

    public override Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        if (_httpContextAccessor.HttpContext.Request.HasFormContentType
        && _httpContextAccessor.HttpContext.Request.Form.TryGetValue(WorkflowConstants.FormLocationKeyInputName, out var value)
        && !string.IsNullOrWhiteSpace(value))
        {
            if (!workflowContext.Output.TryGetValue(WorkflowConstants.HttpFormLocationOutputKeyName, out var obj)
                || obj is not Dictionary<string, string> formLocation)
            {
                formLocation = [];
            }

            formLocation[FormLocationKey] = GetLocationUrl(value);

            workflowContext.Output[WorkflowConstants.HttpFormLocationOutputKeyName] = formLocation;
        }

        return Task.FromResult(Outcomes("Done"));
    }

    public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        => Outcomes(S["Done"]);

    public override ActivityExecutionResult Resume(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        => Outcomes("Done");

    private static string GetLocationUrl(string value)
    {
        if (value.StartsWith('/'))
        {
            return "~" + value;
        }

        return value;
    }
}
