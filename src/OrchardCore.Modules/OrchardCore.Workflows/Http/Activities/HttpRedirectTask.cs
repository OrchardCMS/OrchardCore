using System.Collections.Generic;
using System.Text.Encodings.Web;
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
        private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
        private readonly UrlEncoder _urlEncoder;
        private readonly IStringLocalizer S;

        public HttpRedirectTask(
            IStringLocalizer<HttpRedirectTask> localizer,
            IHttpContextAccessor httpContextAccessor,
            IWorkflowExpressionEvaluator expressionEvaluator,
            UrlEncoder urlEncoder
        )
        {
            S = localizer;
            _httpContextAccessor = httpContextAccessor;
            _expressionEvaluator = expressionEvaluator;
            _urlEncoder = urlEncoder;
        }

        public override string Name => nameof(HttpRedirectTask);

        public override LocalizedString DisplayText => S["Http Redirect Task"];

        public override LocalizedString Category => S["HTTP"];

        public WorkflowExpression<string> Location
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public bool Permanent
        {
            get => GetProperty(() => false);
            set => SetProperty(value);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(S["Done"]);
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var location = await _expressionEvaluator.EvaluateAsync(Location, workflowContext, _urlEncoder);

            _httpContextAccessor.HttpContext.Response.Redirect(location, Permanent);
            _httpContextAccessor.HttpContext.Items[WorkflowHttpResult.Instance] = WorkflowHttpResult.Instance;

            return Outcomes("Done");
        }
    }
}
