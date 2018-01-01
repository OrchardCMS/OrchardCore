using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Activities
{
    public class HttpRedirectTask : TaskActivity
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpRedirectTask(IStringLocalizer<HttpRedirectTask> localizer, IHttpContextAccessor httpContextAccessor)
        {
            T = localizer;
            _httpContextAccessor = httpContextAccessor;
        }

        private IStringLocalizer T { get; }

        public override string Name => nameof(HttpRedirectTask);
        public override LocalizedString Category => T["HTTP"];
        public override LocalizedString Description => T["Redirects the user agent to the specified URL (301/302)."];

        public WorkflowExpression<string> Location
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public WorkflowExpression<bool> Permanent
        {
            get => GetProperty(() => new WorkflowExpression<bool>());
            set => SetProperty(value);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(T["Done"]);
        }

        public override IEnumerable<string> Execute(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            var location = workflowContext.Evaluate(Location);
            var permanent = workflowContext.Evaluate(Permanent);

            _httpContextAccessor.HttpContext.Response.Redirect(location, permanent);
            yield return "Done";
        }
    }
}