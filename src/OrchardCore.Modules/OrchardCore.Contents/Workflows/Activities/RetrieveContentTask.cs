using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Workflows;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Contents.Workflows.Activities
{
    public class RetrieveContentTask : ContentTask
    {
        public RetrieveContentTask(IContentManager contentManager, IWorkflowScriptEvaluator scriptEvaluator, IStringLocalizer<RetrieveContentTask> localizer) : base(contentManager, scriptEvaluator, localizer)
        {
        }

        public override string Name => nameof(RetrieveContentTask);

        public override LocalizedString DisplayText => S["Retrieve Content Task"];

        public override LocalizedString Category => S["Content"];

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(S["Retrieved"]);
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var contentItemId = (await GetContentItemIdAsync(workflowContext))
                ?? throw new InvalidOperationException($"The '{nameof(RetrieveContentTask)}' failed to evaluate the 'ContentItemId'.");

            var contentItem = (await ContentManager.GetAsync(contentItemId, VersionOptions.Latest))
                ?? throw new InvalidOperationException($"The '{nameof(RetrieveContentTask)}' failed to retrieve the content item.");

            workflowContext.CorrelationId = contentItem.ContentItemId;
            workflowContext.Properties[ContentEventConstants.ContentItemInputKey] = contentItem;
            workflowContext.LastResult = contentItem;

            return Outcomes("Retrieved");
        }
    }
}
