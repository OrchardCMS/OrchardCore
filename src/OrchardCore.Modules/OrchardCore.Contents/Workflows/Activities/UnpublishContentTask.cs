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
            return Outcomes(S["Unpublished"]);
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var content = await GetContentAsync(workflowContext);
            await ContentManager.UnpublishAsync(content.ContentItem);

            return Outcomes("Unpublished");
        }
    }
}
