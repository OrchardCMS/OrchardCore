using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Http.Activities
{
    public class HttpRequestFilterEvent : EventActivity
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public static string EventName => nameof(HttpRequestFilterEvent);

        public HttpRequestFilterEvent(IStringLocalizer<HttpRequestFilterEvent> localizer, IHttpContextAccessor httpContextAccessor)
        {
            T = localizer;
            _httpContextAccessor = httpContextAccessor;
        }

        private IStringLocalizer T { get; }

        public override string Name => EventName;
        public override LocalizedString Category => T["HTTP"];

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
            var isHttpMethodMatch = string.Equals(HttpMethod, httpRequest.Method, System.StringComparison.OrdinalIgnoreCase);

            if (!isHttpMethodMatch)
                return false;

            var routeValues = RouteValues;
            var currentRouteValues = httpContext.GetRouteData().Values;
            var isRouteMatch = RouteMatches(routeValues, currentRouteValues);

            if (!isRouteMatch)
                return false;

            return true;
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(T["Matched"]);
        }

        public override ActivityExecutionResult Resume(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes("Matched");
        }

        private void SetRouteValue<T>(string name, T value)
        {
            var routeValues = RouteValues;
            routeValues[name] = value;
            RouteValues = routeValues;
        }

        private bool RouteMatches(RouteValueDictionary a, RouteValueDictionary b)
        {
            return a.All(x =>
            {
                var valueA = x.Value?.ToString();
                return b.ContainsKey(x.Key) && string.Equals(valueA, b[x.Key]?.ToString(), StringComparison.OrdinalIgnoreCase);
            });
        }
    }
}