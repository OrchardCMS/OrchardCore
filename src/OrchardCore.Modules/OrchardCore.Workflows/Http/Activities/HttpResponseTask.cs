using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Primitives;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Http.Activities
{
    public class HttpResponseTask : TaskActivity
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
        private readonly IStringLocalizer S;

        public HttpResponseTask(
            IStringLocalizer<HttpResponseTask> localizer,
            IHttpContextAccessor httpContextAccessor,
            IWorkflowExpressionEvaluator expressionEvaluator
        )
        {
            S = localizer;
            _httpContextAccessor = httpContextAccessor;
            _expressionEvaluator = expressionEvaluator;
        }

        public override string Name => nameof(HttpResponseTask);

        public override LocalizedString DisplayText => S["Http Response Task"];

        public override LocalizedString Category => S["HTTP"];

        public WorkflowExpression<string> Content
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public int HttpStatusCode
        {
            get => GetProperty(() => 200);
            set => SetProperty(value);
        }

        public WorkflowExpression<string> Headers
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public WorkflowExpression<string> ContentType
        {
            get => GetProperty(() => new WorkflowExpression<string>("application/json"));
            set => SetProperty(value);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(S["Done"]);
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var headersString = await _expressionEvaluator.EvaluateAsync(Headers, workflowContext);
            var content = await _expressionEvaluator.EvaluateAsync(Content, workflowContext);
            var contentType = await _expressionEvaluator.EvaluateAsync(ContentType, workflowContext);
            var headers = ParseHeaders(headersString);
            var response = _httpContextAccessor.HttpContext.Response;

            response.StatusCode = HttpStatusCode;

            foreach (var header in headers)
            {
                response.Headers.Add(header);
            }

            if (!string.IsNullOrWhiteSpace(contentType))
            {
                response.ContentType = contentType;
            }

            if (!string.IsNullOrWhiteSpace(content))
            {
                await response.WriteAsync(content);
            }

            _httpContextAccessor.HttpContext.Items[WorkflowHttpResult.Instance] = WorkflowHttpResult.Instance;
            return Outcomes("Done");
        }

        private IEnumerable<KeyValuePair<string, StringValues>> ParseHeaders(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Enumerable.Empty<KeyValuePair<string, StringValues>>();

            return
                from header in text.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim())
                let pair = header.Split(':')
                where pair.Length == 2
                select new KeyValuePair<string, StringValues>(pair[0], pair[1]);
        }
    }
}
