using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Rules.Models;
using OrchardCore.Rules.Services;

namespace OrchardCore.Rules.Drivers;

/// <summary>
/// Saves references to content types which have been displayed during a request.
/// </summary>
public sealed class ContentTypeConditionEvaluatorDriver : ContentDisplayDriver, IConditionEvaluator
{
    private readonly IConditionOperatorResolver _operatorResolver;

    // Hashset to prevent duplicate entries, but comparison is done by the comparers.
    private readonly HashSet<string> _contentTypes = [];

    public ContentTypeConditionEvaluatorDriver(IConditionOperatorResolver operatorResolver)
    {
        _operatorResolver = operatorResolver;
    }

    public override Task<IDisplayResult> DisplayAsync(ContentItem contentItem, BuildDisplayContext context)
    {
        // Do not include Widgets or any display type other than Detail.
        if (context.DisplayType == "Detail" && (!context.Shape.TryGetProperty(nameof(ContentTypeSettings.Stereotype), out string stereotype) || stereotype != "Widget"))
        {
            _contentTypes.Add(contentItem.ContentType);
        }

        return Task.FromResult<IDisplayResult>(null);
    }

    public ValueTask<bool> EvaluateAsync(Condition condition)
        => EvaluateAsync(condition as ContentTypeCondition);

    private ValueTask<bool> EvaluateAsync(ContentTypeCondition condition)
    {
        var operatorComparer = _operatorResolver.GetOperatorComparer(condition.Operation);

        // If no content types are considered, use the empty string as the value to compare against,
        // since we still want comparisons such as "Does Not Equal", "Does Not Start With", etc. to evaluate to true in this case.
        if (_contentTypes.Count == 0)
        {
            return ValueTask.FromResult(operatorComparer.Compare(condition.Operation, string.Empty, condition.Value));
        }

        foreach (var contentType in _contentTypes)
        {
            if (operatorComparer.Compare(condition.Operation, contentType, condition.Value))
            {
                return ValueTask.FromResult(true);
            }
        }

        return ValueTask.FromResult(false);
    }
}
