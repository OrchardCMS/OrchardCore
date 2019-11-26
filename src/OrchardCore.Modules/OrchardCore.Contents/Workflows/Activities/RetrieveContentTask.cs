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
    public class RetrieveContentTask : ContentTask
    {
        public RetrieveContentTask(IContentManager contentManager, IWorkflowScriptEvaluator scriptEvaluator, IStringLocalizer<RetrieveContentTask> localizer) : base(contentManager, scriptEvaluator, localizer)
        {
        }

        public override string Name => nameof(RetrieveContentTask);
        public override LocalizedString DisplayText => T["Retrieve Content Task"];
        public override LocalizedString Category => T["Content"];

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(T["Retrieve"]);
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var contentItemId = await GetContentByIdAsync(workflowContext);

            var contentItem = await ContentManager.GetAsync("", VersionOptions.Published);
            //publishedItem = await GetAsync(contentItem.ContentItemId, VersionOptions.Published);

            return Outcomes("Retrieve");
        }
    }
}
