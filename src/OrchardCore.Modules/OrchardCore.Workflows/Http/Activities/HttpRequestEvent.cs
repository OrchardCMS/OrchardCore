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
        public static string EventName => nameof(HttpRequestEvent);

        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpRequestEvent(
            IStringLocalizer<HttpRequestEvent> localizer,
            IHttpContextAccessor httpContextAccessor
        )
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

        public override bool CanExecute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var httpRequest = httpContext.Request;
            var isMatch = string.Equals(HttpMethod, httpRequest.Method, System.StringComparison.OrdinalIgnoreCase);

            return isMatch;
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(T["Done"]);
        }

        public override ActivityExecutionResult Resume(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes("Done");
        }
    }
}