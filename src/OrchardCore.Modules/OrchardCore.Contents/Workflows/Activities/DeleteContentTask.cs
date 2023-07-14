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
    public class DeleteContentTask : ContentTask
    {
        public DeleteContentTask(IContentManager contentManager, IWorkflowScriptEvaluator scriptEvaluator, IStringLocalizer<DeleteContentTask> localizer) : base(contentManager, scriptEvaluator, localizer)
        {
        }

        public override string Name => nameof(DeleteContentTask);

        public override LocalizedString DisplayText => S["Delete Content Task"];

        public override LocalizedString Category => S["Content"];

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(S["Deleted"], S["Noop"]);
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var content = (await GetContentAsync(workflowContext))
                ?? throw new InvalidOperationException($"The '{nameof(DeleteContentTask)}' failed to retrieve the content item.");

            if (String.Equals(InlineEvent.ContentItemId, content.ContentItem.ContentItemId, StringComparison.OrdinalIgnoreCase))
            {
                return Outcomes("Noop");
            }

            var contentItem = await ContentManager.GetAsync(content.ContentItem.ContentItemId, VersionOptions.Latest);

            if (contentItem == null)
            {
                if (content is ContentItemIdExpressionResult)
                {
                    throw new InvalidOperationException($"The '{nameof(DeleteContentTask)}' failed to retrieve the content item.");
                }

                contentItem = content.ContentItem;
            }

            if (InlineEvent.IsStart && InlineEvent.ContentType == contentItem.ContentType && InlineEvent.Name == nameof(ContentDeletedEvent))
            {
                throw new InvalidOperationException($"The '{nameof(DeleteContentTask)}' can't delete the content item as it is executed inline from a starting '{nameof(ContentDeletedEvent)}' of the same content type, which would result in an infinitive loop.");
            }

            await ContentManager.RemoveAsync(contentItem);

            return Outcomes("Deleted");
        }
    }
}
