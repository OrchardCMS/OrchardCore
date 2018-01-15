using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Http.Activities
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
            get => GetProperty(() => new WorkflowExpression<bool>("false"));
            set => SetProperty(value);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(T["Done"]);
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var locationTask = workflowContext.EvaluateExpressionAsync(Location);
            var permanentTask = workflowContext.EvaluateScriptAsync(Permanent);

            await Task.WhenAll(locationTask, permanentTask);

            _httpContextAccessor.HttpContext.Response.Redirect(locationTask.Result, permanentTask.Result);
            return Outcomes("Done");
        }
    }
}