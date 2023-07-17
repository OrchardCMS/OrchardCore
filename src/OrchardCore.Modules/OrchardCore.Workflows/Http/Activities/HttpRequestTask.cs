using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
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
    public class HttpRequestTask : TaskActivity
    {
        private static readonly Dictionary<int, string> _httpStatusCodeDictionary = new()
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

        private static readonly HttpClient _httpClient = new();
        private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
        protected readonly IStringLocalizer S;
        private readonly UrlEncoder _urlEncoder;

        public HttpRequestTask(
            IStringLocalizer<HttpRequestTask> localizer,
            IWorkflowExpressionEvaluator expressionEvaluator,
            UrlEncoder urlEncoder
        )
        {
            S = localizer;
            _expressionEvaluator = expressionEvaluator;
            _urlEncoder = urlEncoder;
        }

        public override string Name => nameof(HttpRequestTask);

        public override LocalizedString DisplayText => S["Http Request Task"];

        public override LocalizedString Category => S["HTTP"];

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
            var outcomes = !String.IsNullOrWhiteSpace(HttpResponseCodes)
                ? HttpResponseCodes.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x =>
                {
                    var status = Int32.Parse(x.Trim());

                    var description = _httpStatusCodeDictionary.TryGetValue(status, out var text)
                        ? $"{status} {text}"
                        : status.ToString()
                        ;

                    return new Outcome(status.ToString(), new LocalizedString(description, description));
                }).ToList()
                : new List<Outcome>();
            outcomes.Add(new Outcome("UnhandledHttpStatus", S["Unhandled Http Status"]));

            return outcomes;
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var headersText = await _expressionEvaluator.EvaluateAsync(Headers, workflowContext, _urlEncoder);
            var headers = ParseHeaders(headersText);

            var httpMethod = HttpMethod;
            var url = await _expressionEvaluator.EvaluateAsync(Url, workflowContext, _urlEncoder);
            var request = new HttpRequestMessage(new HttpMethod(httpMethod), url);
            foreach (var header in headers)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            if (HttpMethods.IsPatch(httpMethod) || HttpMethods.IsPost(httpMethod) || HttpMethods.IsPut(httpMethod))
            {
                var body = await _expressionEvaluator.EvaluateAsync(Body, workflowContext, null);
                var contentType = await _expressionEvaluator.EvaluateAsync(ContentType, workflowContext, _urlEncoder);
                request.Content = new StringContent(body, Encoding.UTF8, contentType);
            }

            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);
            var responseCodes = ParseResponseCodes(HttpResponseCodes);
            var outcome = responseCodes.FirstOrDefault(x => x == (int)response.StatusCode);

            workflowContext.LastResult = new
            {
                Body = await response.Content.ReadAsStringAsync(),
                Headers = response.Headers.ToDictionary(x => x.Key),
                response.StatusCode,
                response.ReasonPhrase,
                response.IsSuccessStatusCode
            };

            return Outcomes(outcome != 0 ? outcome.ToString() : "UnhandledHttpStatus");
        }

        private static IEnumerable<KeyValuePair<string, string>> ParseHeaders(string text)
        {
            if (String.IsNullOrWhiteSpace(text))
                return Enumerable.Empty<KeyValuePair<string, string>>();

            return
                from header in text.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim())
                let pair = header.Split(':', 2)
                where pair.Length == 2
                select new KeyValuePair<string, string>(pair[0], pair[1]);
        }

        private static IEnumerable<int> ParseResponseCodes(string text)
        {
            return
                from code in text.Split(',', StringSplitOptions.RemoveEmptyEntries)
                select Int32.Parse(code);
        }
    }
}
