using System.Net.Mime;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Http.Services;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Http.Activities;

public class HttpRequestTask : TaskActivity<HttpRequestTask>
{
    private static readonly string[] s_separator = ["\r\n", "\n", "\r"];

    private static readonly Dictionary<int, string> s_httpStatusCodeDictionary = new()
    {
        { 100, "Continue" },
        { 101, "Switching Protocols" },
        { 102, "Processing" },
        { 200, "OK" },
        { 201, "Created" },
        { 202, "Accepted" },
        { 203, "Non-authoritative Information" },
        { 204, "No Content" },
        { 205, "Reset Content" },
        { 206, "Partial Content" },
        { 207, "Multi-Status" },
        { 208, "Already Reported" },
        { 226, "IM Used" },
        { 300, "Multiple Choices" },
        { 301, "Moved Permanently" },
        { 302, "Found" },
        { 303, "See Other" },
        { 304, "Not Modified" },
        { 305, "Use Proxy" },
        { 307, "Temporary Redirect" },
        { 308, "Permanent Redirect" },
        { 400, "Bad Request" },
        { 401, "Unauthorized" },
        { 402, "Payment Required" },
        { 403, "Forbidden" },
        { 404, "Not Found" },
        { 405, "Method Not Allowed" },
        { 406, "Not Acceptable" },
        { 407, "Proxy Authentication Required" },
        { 408, "Request Timeout" },
        { 409, "Conflict" },
        { 410, "Gone" },
        { 411, "Length Required" },
        { 412, "Precondition Failed" },
        { 413, "Payload Too Large" },
        { 414, "Request-URI Too Long" },
        { 415, "Unsupported Media Type" },
        { 416, "Requested Range Not Satisfiable" },
        { 417, "Expectation Failed" },
        { 418, "I'm a teapot" },
        { 421, "Misdirected Request" },
        { 422, "Unprocessable Entity" },
        { 423, "Locked" },
        { 424, "Failed Dependency" },
        { 426, "Upgrade Required" },
        { 428, "Precondition Required" },
        { 429, "Too Many Requests" },
        { 431, "Request Header Fields Too Large" },
        { 444, "Connection Closed Without Response" },
        { 451, "Unavailable For Legal Reasons" },
        { 499, "Client Closed Request" },
        { 500, "Internal Server Error" },
        { 501, "Not Implemented" },
        { 502, "Bad Gateway" },
        { 503, "Service Unavailable" },
        { 504, "Gateway Timeout" },
        { 505, "HTTP Version Not Supported" },
        { 506, "Variant Also Negotiates" },
        { 507, "Insufficient Storage" },
        { 508, "Loop Detected" },
        { 510, "Not Extended" },
        { 511, "Network Authentication Required" },
        { 599, "Network Connect Timeout Error" },
    };

    private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly UrlEncoder _urlEncoder;
    private readonly ILogger _logger;

    protected readonly IStringLocalizer S;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpRequestTask"/> class.
    /// </summary>
    /// <param name="expressionEvaluator">The workflow expression evaluator.</param>
    /// <param name="urlEncoder">The URL encoder.</param>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    /// <param name="localizer">The string localizer.</param>
    /// <param name="logger">The logger.</param>
    public HttpRequestTask(
        IWorkflowExpressionEvaluator expressionEvaluator,
        UrlEncoder urlEncoder,
        IHttpClientFactory httpClientFactory,
        IStringLocalizer<HttpRequestTask> localizer,
        ILogger<HttpRequestTask> logger
    )
    {
        _expressionEvaluator = expressionEvaluator;
        _urlEncoder = urlEncoder;
        _httpClientFactory = httpClientFactory;
        S = localizer;
        _logger = logger;
    }

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
        get => GetProperty(() => new WorkflowExpression<string>(MediaTypeNames.Application.Json));
        set => SetProperty(value);
    }

    public string HttpResponseCodes
    {
        get => GetProperty(() => "200");
        set => SetProperty(value);
    }

    public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        var outcomes = !string.IsNullOrWhiteSpace(HttpResponseCodes)
            ? HttpResponseCodes.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x =>
            {
                var status = int.Parse(x.Trim());

                var description = s_httpStatusCodeDictionary.TryGetValue(status, out var text)
                    ? $"{status} {text}"
                    : status.ToString()
                    ;

                return new Outcome(status.ToString(), new LocalizedString(description, description));
            }).ToList()
            : [];
        outcomes.Add(new Outcome("UnhandledHttpStatus", S["Unhandled Http Status"]));

        return outcomes;
    }

    public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        var headersText = await _expressionEvaluator.EvaluateAsync(Headers, workflowContext, _urlEncoder);
        var headers = ParseHeaders(headersText);

        var httpMethod = HttpMethod;
        var url = await _expressionEvaluator.EvaluateAsync(Url, workflowContext, _urlEncoder);

        if (!HttpRequestDestinationValidator.TryCreateUri(url, out var destination, out var failureReason))
        {
            _logger.LogWarning("The HTTP request workflow task did not send a request because its destination was invalid: {FailureReason}", failureReason);
            return Outcome("UnhandledHttpStatus");
        }

        if (headers.Any(header => string.Equals(header.Key, "Host", StringComparison.OrdinalIgnoreCase)))
        {
            _logger.LogWarning("The HTTP request workflow task did not send a request because overriding the Host header is not allowed.");
            return Outcome("UnhandledHttpStatus");
        }

        using var request = new HttpRequestMessage(new HttpMethod(httpMethod), destination);
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

        var httpClient = _httpClientFactory.CreateClient(HttpRequestTaskHttpClient.Name);

        HttpResponseMessage response;

        try
        {
            response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead, workflowContext.CancellationToken);
        }
        catch (HttpRequestException exception) when (ContainsBlockedDestinationException(exception))
        {
            _logger.LogWarning("The HTTP request workflow task did not send a request because host '{Host}' resolved to a prohibited destination.", destination.IdnHost);
            return Outcome("UnhandledHttpStatus");
        }

        using (response)
        {
            var responseCodes = ParseResponseCodes(HttpResponseCodes);

            var outcome = responseCodes.FirstOrDefault(x => x == (int)response.StatusCode);

            workflowContext.LastResult = new
            {
                Body = await response.Content.ReadAsStringAsync(workflowContext.CancellationToken),
                Headers = response.Headers.ToDictionary(x => x.Key),
                response.StatusCode,
                response.ReasonPhrase,
                response.IsSuccessStatusCode,
            };

            return Outcome(outcome != 0 ? outcome.ToString() : "UnhandledHttpStatus");
        }
    }

    private static IEnumerable<KeyValuePair<string, string>> ParseHeaders(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        return
            from header in text.Split(s_separator, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim())
            let pair = header.Split(':', 2)
            where pair.Length == 2
            select new KeyValuePair<string, string>(pair[0], pair[1]);
    }

    private static IEnumerable<int> ParseResponseCodes(string text)
    {
        return
            from code in text.Split(',', StringSplitOptions.RemoveEmptyEntries)
            select int.Parse(code);
    }

    private static bool ContainsBlockedDestinationException(Exception exception)
    {
        while (exception != null)
        {
            if (exception is HttpRequestDestinationNotAllowedException)
            {
                return true;
            }

            exception = exception.InnerException;
        }

        return false;
    }
}
