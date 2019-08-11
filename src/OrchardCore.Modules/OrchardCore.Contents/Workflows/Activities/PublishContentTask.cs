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
        public override LocalizedString Category => T["Content"];

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(T["Published"]);
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var content = await GetContentAsync(workflowContext);
            await ContentManager.PublishAsync(content.ContentItem);
            return Outcomes("Published");
        }
    }
}