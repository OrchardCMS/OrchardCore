using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Http.Activities
{
    public class HttpRequestTask : TaskActivity
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWorkflowExpressionEvaluator _expressionEvaluator;

        public HttpRequestTask(
            IStringLocalizer<HttpRequestTask> localizer,
            IHttpContextAccessor httpContextAccessor,
            IWorkflowExpressionEvaluator expressionEvaluator
        )
        {
            T = localizer;
            _httpContextAccessor = httpContextAccessor;
            _expressionEvaluator = expressionEvaluator;
        }

        private IStringLocalizer T { get; }

        public override string Name => nameof(HttpRequestTask);
        public override LocalizedString Category => T["HTTP"];

        public WorkflowExpression<string> Url
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public string HttpMethod
        {
            get => GetProperty(() => HttpMethods.Get);
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
            var httpStatusCodeDictionary = new Dictionary<int, string>
            {
                { 100, "Continue" },
                { 101, " Switching Protocols" },
                { 102 , "Processing" },
                { 200 , "OK" },
                { 201 , "Created" },
                { 202 , "Accepted" },
                { 203 , "Non-authoritative Information" },
                { 204 , "No Content" },
                { 205 , "Reset Content" },
                { 206 , "Partial Content" },
                { 207 , "Multi-Status" },
                { 208 , "Already Reported" },
                { 226 , "IM Used" },
                { 300 , "Multiple Choices" },
                { 301 , "Moved Permanently" },
                { 302 , "Found" },
                { 303 , "See Other" },
                { 304 , "Not Modified" },
                { 305 , "Use Proxy" },
                { 307 , "Temporary Redirect" },
                { 308 , "Permanent Redirect" },
                { 400 , "Bad Request" },
                { 401 , "Unauthorized" },
                { 402 , "Payment Required" },
                { 403 , "Forbidden" },
                { 404 , "Not Found" },
                { 405 , "Method Not Allowed" },
                { 406 , "Not Acceptable" },
                { 407 , "Proxy Authentication Required" },
                { 408 , "Request Timeout" },
                { 409 , "Conflict" },
                { 410 , "Gone" },
                { 411 , "Length Required" },
                { 412 , "Precondition Failed" },
                { 413 , "Payload Too Large" },
                { 414 , "Request-URI Too Long" },
                { 415 , "Unsupported Media Type" },
                { 416 , "Requested Range Not Satisfiable" },
                { 417 , "Expectation Failed" },
                { 418 , "I'm a teapot" },
                { 421 , "Misdirected Request" },
                { 422 , "Unprocessable Entity" },
                { 423 , "Locked" },
                { 424 , "Failed Dependency" },
                { 426 , "Upgrade Required" },
                { 428 , "Precondition Required" },
                { 429 , "Too Many Requests" },
                { 431 , "Request Header Fields Too Large" },
                { 444 , "Connection Closed Without Response" },
                { 451 , "Unavailable For Legal Reasons" },
                { 499 , "Client Closed Request" },
                { 500 , "Internal Server Error" },
                { 501 , "Not Implemented" },
                { 502 , "Bad Gateway" },
                { 503 , "Service Unavailable" },
                { 504 , "Gateway Timeout" },
                { 505 , "HTTP Version Not Supported" },
                { 506 , "Variant Also Negotiates" },
                { 507 , "Insufficient Storage" },
                { 508 , "Loop Detected" },
                { 510 , "Not Extended" },
                { 511 , "Network Authentication Required" },
                { 599 , "Network Connect Timeout Error" }
            };
            var outcomes = !string.IsNullOrWhiteSpace(HttpResponseCodes)
                ? HttpResponseCodes.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x =>
                {
                    var status = int.Parse(x.Trim());
                    var description = httpStatusCodeDictionary.ContainsKey(status) ? $"{status} {httpStatusCodeDictionary[status]}" : status.ToString();
                    return new Outcome(status.ToString(), T[description]);
                }).ToList()
                : new List<Outcome>();
            outcomes.Add(new Outcome("UnhandledHttpStatus", T["Unhandled Http Status"]));

            return outcomes;
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            // TODO: Refactor this to using HttpClientFactory.
            using (var httpClient = new HttpClient())
            {
                var headersText = await _expressionEvaluator.EvaluateAsync(Headers, workflowContext);
                var headers = ParseHeaders(headersText);

                foreach (var header in headers)
                {
                    httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                }

                var httpMethod = HttpMethod;
                var url = await _expressionEvaluator.EvaluateAsync(Url, workflowContext);
                var request = new HttpRequestMessage(new HttpMethod(httpMethod), url);
                var postMethods = new[] { HttpMethods.Patch, HttpMethods.Post, HttpMethods.Put };

                if (postMethods.Any(x => string.Equals(x, httpMethod, StringComparison.OrdinalIgnoreCase)))
                {
                    var body = await _expressionEvaluator.EvaluateAsync(Body, workflowContext);
                    var contentType = await _expressionEvaluator.EvaluateAsync(ContentType, workflowContext);
                    request.Content = new StringContent(body, Encoding.UTF8, contentType);
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