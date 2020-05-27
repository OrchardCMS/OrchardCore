using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Contents.Workflows.Activities
{
    public class UnpublishContentTask : ContentTask
    {
        public UnpublishContentTask(IContentManager contentManager, IWorkflowScriptEvaluator scriptEvaluator, IStringLocalizer<UnpublishContentTask> localizer) : base(contentManager, scriptEvaluator, localizer)
        {
        }

        public override string Name => nameof(UnpublishContentTask);

        public override LocalizedString DisplayText => S["Unpublish Content Task"];

        public override LocalizedString Category => S["Content"];

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(S["Unpublished"], S["Noop"]);
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var content = await GetContentAsync(workflowContext);

            if (content == null)
            {
                throw new InvalidOperationException($"The {workflowContext.WorkflowType.Name}:{DisplayText} activity failed to retrieve the content item.");
            }

            if (IsInlineFromContentEventOfSameContentItemId(content.ContentItem.ContentItemId))
            {
                return Outcomes("Noop");
            }

            var contentItem = await ContentManager.GetAsync(content.ContentItem.ContentItemId, VersionOptions.Latest);

            if (contentItem == null)
            {
                if (content is ContentItemIdExpressionContent)
                {
                    throw new InvalidOperationException($"The {workflowContext.WorkflowType.Name}:{DisplayText} activity failed to retrieve the content item.");
                }

                contentItem = content.ContentItem;
            }

            if (IsInlineFromStartingContentEventOfSameContentType(contentItem.ContentType))
            {
                if (ContentEventInfo.Name == nameof(ContentUnpublishedEvent))
                {
                    throw new InvalidOperationException($"The '{workflowContext.WorkflowType.Name}:{DisplayText}' can't unpublish the content item as it is executed inline from a starting '{nameof(ContentUnpublishedEvent.DisplayText)}' of the same content type.");
                }
            }

            await ContentManager.UnpublishAsync(contentItem);

            return Outcomes("Unpublished");
        }
    }
}
