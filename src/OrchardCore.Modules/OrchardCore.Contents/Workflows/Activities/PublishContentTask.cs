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
    public class PublishContentTask : ContentTask
    {
        public PublishContentTask(IContentManager contentManager, IWorkflowScriptEvaluator scriptEvaluator, IStringLocalizer<PublishContentTask> localizer) : base(contentManager, scriptEvaluator, localizer)
        {
        }

        public override string Name => nameof(PublishContentTask);

        public override LocalizedString DisplayText => S["Publish Content Task"];

        public override LocalizedString Category => S["Content"];

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(S["Published"], S["Noop"]);
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var content = (await GetContentAsync(workflowContext))
                ?? throw new InvalidOperationException($"The '{nameof(PublishContentTask)}' failed to retrieve the content item.");

            if (String.Equals(InlineEvent.ContentItemId, content.ContentItem.ContentItemId, StringComparison.OrdinalIgnoreCase))
            {
                return Outcomes("Noop");
            }

            var contentItem = await ContentManager.GetAsync(content.ContentItem.ContentItemId, VersionOptions.DraftRequired);

            if (contentItem == null)
            {
                if (content is ContentItemIdExpressionResult)
                {
                    throw new InvalidOperationException($"The '{nameof(PublishContentTask)}' failed to retrieve the content item.");
                }

                contentItem = content.ContentItem;
            }

            if (InlineEvent.IsStart && InlineEvent.ContentType == contentItem.ContentType && InlineEvent.Name == nameof(ContentPublishedEvent))
            {
                throw new InvalidOperationException($"The '{nameof(PublishContentTask)}' can't publish the content item as it is executed inline from a starting '{nameof(ContentPublishedEvent)}' of the same content type, which would result in an infinitive loop.");
            }

            await ContentManager.PublishAsync(contentItem);

            return Outcomes("Published");
        }
    }
}
