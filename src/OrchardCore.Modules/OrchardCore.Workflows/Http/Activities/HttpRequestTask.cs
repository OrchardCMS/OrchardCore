using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Linq;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Http.Activities
{
    public class HttpRequestTask : TaskActivity
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpRequestTask(IStringLocalizer<HttpRequestTask> localizer, IHttpContextAccessor httpContextAccessor)
        {
            T = localizer;
            _httpContextAccessor = httpContextAccessor;
        }

        private IStringLocalizer T { get; }

        public override string Name => nameof(HttpRequestTask);
        public override LocalizedString Category => T["HTTP"];
        public override LocalizedString Description => T["Makes a HTTP request to the specified URL."];

        public WorkflowExpression<string> Url
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public WorkflowExpression<string> HttpMethod
        {
            get => GetProperty(() => new WorkflowExpression<string>(HttpMethods.Get));
            set => SetProperty(value);
        }

        public WorkflowExpression<string> Headers
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public WorkflowExpression<string> Body
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public WorkflowExpression<string> ContentType
        {
            get => GetProperty(() => new WorkflowExpression<string>("application/json"));
            set => SetProperty(value);
        }

        public string HttpResponseCodes
        {
            get => GetProperty(() => "200");
            set => SetProperty(value);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var outcomes = !string.IsNullOrWhiteSpace(HttpResponseCodes) ? HttpResponseCodes.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => new Outcome(T[x.Trim()])).ToList() : new List<Outcome>();
            outcomes.Add(new Outcome("UnhandledHttpStatus", T["Unhandled Http Status"]));

            return outcomes;
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            using (var httpClient = new HttpClient())
            {
                var headersText = await workflowContext.EvaluateExpressionAsync(Headers);
                var headers = ParseHeaders(headersText);

                foreach (var header in headers)
                {
                    httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                }

                var httpMethod = await workflowContext.EvaluateExpressionAsync(HttpMethod);
                var url = await workflowContext.EvaluateExpressionAsync(Url);
                var request = new HttpRequestMessage(new HttpMethod(httpMethod), url);
                var postMethods = new[] { HttpMethods.Patch, HttpMethods.Post, HttpMethods.Put };

                if (postMethods.Any(x => string.Equals(x, httpMethod, StringComparison.OrdinalIgnoreCase)))
                {
                    var body = await workflowContext.EvaluateExpressionAsync(Body);
                    var contentType = await workflowContext.EvaluateExpressionAsync(ContentType);
                    var content = new StringContent(body, Encoding.UTF8, contentType);
                }

                var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);
                var responseCodes = ParseResponseCodes(HttpResponseCodes);
                var outcome = responseCodes.FirstOrDefault(x => x == (int)response.StatusCode);

                workflowContext.LastResult = new
                {
                    Body = await response.Content.ReadAsStringAsync(),
                    Headers = response.Headers.ToDictionary(x => x.Key),
                    StatusCode = response.StatusCode,
                    ReasonPhrase = response.ReasonPhrase,
                    IsSuccessStatusCode = response.IsSuccessStatusCode
                };

                return Outcomes(outcome != 0 ? outcome.ToString() : "UnhandledHttpStatus");
            }
        }

        private IEnumerable<KeyValuePair<string, string>> ParseHeaders(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Enumerable.Empty<KeyValuePair<string, string>>();

            return
                from header in text.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim())
                let pair = header.Split(new[] { ':' })
                where pair.Length == 2
                select new KeyValuePair<string, string>(pair[0], pair[1]);
        }

        private IEnumerable<int> ParseResponseCodes(string text)
        {
            return
                from code in text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                select int.Parse(code);
        }
    }
}