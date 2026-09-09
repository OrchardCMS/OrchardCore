using Microsoft.Extensions.Localization;
using OrchardCore.Facebook.Models;
using OrchardCore.Facebook.Services;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Facebook.Activities;

/// <summary>
/// A workflow task that sends a server-side event to the Meta Conversions API, e.g. to report a
/// completed order or a captured lead directly from the server, independently of (and optionally
/// deduplicated with) the browser-side Meta Pixel.
/// </summary>
public sealed class MetaConversionsApiEventTask : TaskActivity<MetaConversionsApiEventTask>
{
    private readonly IMetaConversionsApiService _metaConversionsApiService;
    private readonly IWorkflowExpressionEvaluator _expressionEvaluator;

    internal readonly IStringLocalizer S;

    public MetaConversionsApiEventTask(
        IMetaConversionsApiService metaConversionsApiService,
        IWorkflowExpressionEvaluator expressionEvaluator,
        IStringLocalizer<MetaConversionsApiEventTask> stringLocalizer)
    {
        _metaConversionsApiService = metaConversionsApiService;
        _expressionEvaluator = expressionEvaluator;
        S = stringLocalizer;
    }

    public override LocalizedString DisplayText => S["Meta Conversions API Event Task"];

    public override LocalizedString Category => S["Meta"];

    public WorkflowExpression<string> EventName
    {
        get => GetProperty(() => new WorkflowExpression<string>());
        set => SetProperty(value);
    }

    public WorkflowExpression<string> EventSourceUrl
    {
        get => GetProperty(() => new WorkflowExpression<string>());
        set => SetProperty(value);
    }

    public MetaActionSource ActionSource
    {
        get => GetProperty(() => MetaActionSource.Website);
        set => SetProperty(value);
    }

    public WorkflowExpression<string> EventId
    {
        get => GetProperty(() => new WorkflowExpression<string>());
        set => SetProperty(value);
    }

    public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        => Outcome(S["Done"], S["Failed"]);

    public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        var eventName = await _expressionEvaluator.EvaluateAsync(EventName, workflowContext, null);
        var eventSourceUrl = await _expressionEvaluator.EvaluateAsync(EventSourceUrl, workflowContext, null);
        var eventId = await _expressionEvaluator.EvaluateAsync(EventId, workflowContext, null);

        var result = await _metaConversionsApiService.SendEventAsync(new MetaConversionEvent
        {
            EventName = eventName,
            EventSourceUrl = string.IsNullOrWhiteSpace(eventSourceUrl) ? null : eventSourceUrl,
            ActionSource = ActionSource,
            EventId = string.IsNullOrWhiteSpace(eventId) ? null : eventId,
        });

        workflowContext.LastResult = result;

        if (result.Succeeded)
        {
            return Outcome("Done");
        }

        return Outcome("Failed");
    }
}
