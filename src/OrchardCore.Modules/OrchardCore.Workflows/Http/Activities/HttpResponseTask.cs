using System.Net.Mime;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Primitives;
using OrchardCore.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Http.Activities;

public class HttpResponseTask : TaskActivity<HttpResponseTask>, IActivityDescription
{
    private static readonly string[] s_separator = ["\r\n", "\n", "\r"];

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
    private readonly IStringLocalizerFactory _localizerFactory;
    protected readonly IStringLocalizer S;
    private readonly UrlEncoder _urlEncoder;

    public HttpResponseTask(
        IStringLocalizerFactory localizerFactory,
        IStringLocalizer<HttpResponseTask> localizer,
        IHttpContextAccessor httpContextAccessor,
        IWorkflowExpressionEvaluator expressionEvaluator,
        UrlEncoder urlEncoder
    )
    {
        _localizerFactory = localizerFactory;
        S = localizer;
        _httpContextAccessor = httpContextAccessor;
        _expressionEvaluator = expressionEvaluator;
        _urlEncoder = urlEncoder;
    }

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<HttpResponseTask>("Http Response Task");

    public static LocalizationSource CategoryName => ActivityCategoryLocalizationSources.Http;

    public override LocalizedString DisplayText => _localizerFactory.Localize(DisplayName);

    public override LocalizedString Category => _localizerFactory.Localize(CategoryName);

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
        get => GetProperty(() => new WorkflowExpression<string>(MediaTypeNames.Application.Json));
        set => SetProperty(value);
    }

    public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        => Outcome(S["Done"]);

    public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        var headersString = await _expressionEvaluator.EvaluateAsync(Headers, workflowContext, _urlEncoder);
        var content = await _expressionEvaluator.EvaluateAsync(Content, workflowContext, null);
        var contentType = await _expressionEvaluator.EvaluateAsync(ContentType, workflowContext, _urlEncoder);
        var headers = ParseHeaders(headersString);
        var response = _httpContextAccessor.HttpContext.Response;

        response.StatusCode = HttpStatusCode;

        foreach (var header in headers)
        {
            response.Headers.Append(header.Key, header.Value);
        }

        if (!string.IsNullOrWhiteSpace(contentType))
        {
            response.ContentType = contentType;
        }

        if (!string.IsNullOrWhiteSpace(content))
        {
            await response.WriteAsync(content, workflowContext.CancellationToken);
        }

        _httpContextAccessor.HttpContext.Items[WorkflowHttpResult.Instance] = WorkflowHttpResult.Instance;

        return Outcome("Done");
    }

    private static IEnumerable<KeyValuePair<string, StringValues>> ParseHeaders(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        return
            from header in text.Split(s_separator, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim())
            let pair = header.Split(':')
            where pair.Length == 2
            select new KeyValuePair<string, StringValues>(pair[0], pair[1]);
    }
}
