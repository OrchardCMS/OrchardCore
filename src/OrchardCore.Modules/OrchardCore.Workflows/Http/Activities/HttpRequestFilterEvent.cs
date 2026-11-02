using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Http.Activities;

public class HttpRequestFilterEvent : EventActivity, IActivityDescription
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IStringLocalizerFactory _localizerFactory;
    protected readonly IStringLocalizer S;

    public static string EventName => nameof(HttpRequestFilterEvent);

    public HttpRequestFilterEvent(IStringLocalizerFactory localizerFactory, IStringLocalizer<HttpRequestFilterEvent> localizer, IHttpContextAccessor httpContextAccessor)
    {
        _localizerFactory = localizerFactory;
        S = localizer;
        _httpContextAccessor = httpContextAccessor;
    }

    public override string Name => EventName;
    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<HttpRequestFilterEvent>("Http Request Filter Event");

    public static LocalizationSource CategoryName => ActivityCategoryLocalizationSources.Http;

    public override LocalizedString DisplayText => _localizerFactory.Localize(DisplayName);

    public override LocalizedString Category => _localizerFactory.Localize(CategoryName);

    public string HttpMethod
    {
        get => GetProperty<string>();
        set => SetProperty(value);
    }

    public string ControllerName
    {
        get => (string)RouteValues["controller"];
        set => SetRouteValue("controller", value);
    }

    public string ActionName
    {
        get => (string)RouteValues["action"];
        set => SetRouteValue("action", value);
    }

    public string AreaName
    {
        get => (string)RouteValues["area"];
        set => SetRouteValue("area", value);
    }

    public RouteValueDictionary RouteValues
    {
        get => GetProperty(() => new RouteValueDictionary());
        set => SetProperty(value);
    }

    public override bool CanExecute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var httpRequest = httpContext.Request;
        var isHttpMethodMatch = string.Equals(HttpMethod, httpRequest.Method, StringComparison.OrdinalIgnoreCase);

        if (!isHttpMethodMatch)
        {
            return false;
        }

        var routeValues = RouteValues;
        var currentRouteValues = httpContext.Request.RouteValues;
        var isRouteMatch = RouteMatches(routeValues, currentRouteValues);

        if (!isRouteMatch)
        {
            return false;
        }

        return true;
    }

    public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        => Outcome(S["Matched"]);

    public override ActivityExecutionResult Resume(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        => Outcome("Matched");

    private void SetRouteValue<T>(string name, T value)
    {
        var routeValues = RouteValues;
        routeValues[name] = value;
        RouteValues = routeValues;
    }

    private static bool RouteMatches(RouteValueDictionary a, RouteValueDictionary b)
    {
        return a.All(x =>
        {
            var valueA = x.Value?.ToString();
            return b.ContainsKey(x.Key) && string.Equals(valueA, b[x.Key]?.ToString(), StringComparison.OrdinalIgnoreCase);
        });
    }
}
