using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Contents.Workflows.Activities;

public class UnpublishContentTask : ContentTask, IActivityDescription
{
    public UnpublishContentTask(
        IContentManager contentManager,
        IWorkflowScriptEvaluator scriptEvaluator,
        IStringLocalizerFactory localizerFactory,
        IStringLocalizer<UnpublishContentTask> localizer)
        : base(contentManager, scriptEvaluator, localizerFactory, localizer)
    {
    }

    public override string Name => nameof(UnpublishContentTask);

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<UnpublishContentTask>("Unpublish Content Task");

    public override LocalizedString DisplayText => LocalizerFactory.Localize(DisplayName);

    public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        => Outcome(S["Unpublished"], S["Noop"]);

    public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        var content = (await GetContentAsync(workflowContext))
            ?? throw new InvalidOperationException($"The '{nameof(UnpublishContentTask)}' failed to retrieve the content item.");

        if (string.Equals(InlineEvent.ContentItemId, content.ContentItem.ContentItemId, StringComparison.OrdinalIgnoreCase))
        {
            return Outcome("Noop");
        }

        var contentItem = await ContentManager.GetAsync(content.ContentItem.ContentItemId, VersionOptions.Latest);

        if (contentItem == null)
        {
            if (content is ContentItemIdExpressionResult)
            {
                throw new InvalidOperationException($"The '{nameof(UnpublishContentTask)}' failed to retrieve the content item.");
            }

            contentItem = content.ContentItem;
        }

        if (InlineEvent.IsStart && InlineEvent.ContentType == contentItem.ContentType && InlineEvent.Name == nameof(ContentUnpublishedEvent))
        {
            throw new InvalidOperationException($"The '{nameof(UnpublishContentTask)}' can't unpublish the content item as it is executed inline from a starting '{nameof(ContentUnpublishedEvent)}' of the same content type, which would result in an infinitive loop.");
        }

        await ContentManager.UnpublishAsync(contentItem);

        return Outcome("Unpublished");
    }
}
