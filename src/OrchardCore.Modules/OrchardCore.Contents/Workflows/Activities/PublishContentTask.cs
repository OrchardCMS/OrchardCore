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
            var content = await GetContentAsync(workflowContext);

            if (content == null)
            {
                throw new InvalidOperationException($"The {workflowContext.WorkflowType.Name}:{DisplayText} activity failed to retrieve the content item.");
            }

            if (AsContentDriverOrHandler)
            {
                return Outcomes("Noop");
            }

            var contentItem = await ContentManager.GetAsync(content.ContentItem.ContentItemId, VersionOptions.DraftRequired);

            if (contentItem != null)
            {
                await ContentManager.PublishAsync(contentItem);
            }
            else if (content is ContentItemIdExpressionContent)
            {
                throw new InvalidOperationException($"The {workflowContext.WorkflowType.Name}:{DisplayText} activity failed to retrieve the content item.");
            }
            else
            {
                await ContentManager.PublishAsync(content.ContentItem);
            }

            return Outcomes("Published");
        }
    }
}
