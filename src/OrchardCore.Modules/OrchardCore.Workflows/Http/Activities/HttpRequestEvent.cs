using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Http.Activities
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
        public override LocalizedString Category => T["HTTP"];
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

        public override bool CanExecute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var httpRequest = httpContext.Request;

            var isMatch =
                string.Equals(HttpMethod, httpRequest.Method, System.StringComparison.OrdinalIgnoreCase)
                && string.Equals(RequestPath, httpRequest.Path.Value, System.StringComparison.OrdinalIgnoreCase);

            return isMatch;

        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(T["Matched"]);
        }

        public override ActivityExecutionResult Resume(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes("Matched");
        }
    }
}