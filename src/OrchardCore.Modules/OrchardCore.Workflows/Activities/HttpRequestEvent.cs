using System.Collections.Generic;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Activities
{
    public class HttpRequestEvent : EventActivity
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public static string EventName => nameof(HttpRequestEvent);

        public HttpRequestEvent(IStringLocalizer<HttpRequestEvent> localizer, IHttpContextAccessor httpContextAccessor)
        {
            T = localizer;
            _httpContextAccessor = httpContextAccessor;
        }

        private IStringLocalizer T { get; }

        public override string Name => EventName;
        public override LocalizedString Category => T["Networking"];
        public override LocalizedString Description => T["Executes when the specified HTTP request comes in."];

        public string HttpMethod
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public string RequestPath
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public override bool CanExecute(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var httpRequest = httpContext.Request;

            var isMatch =
                string.Equals(HttpMethod, httpRequest.Method, System.StringComparison.OrdinalIgnoreCase)
                && string.Equals(RequestPath, httpRequest.Path.Value, System.StringComparison.OrdinalIgnoreCase);

            return isMatch;

        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(T["Matched"]);
        }

        public override IEnumerable<string> Execute(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            yield return "Matched";
        }
    }
}