using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Contents.Workflows.Activities
{
    public class PublishContentTask : ContentTask
    {
        public PublishContentTask(IContentManager contentManager, IStringLocalizer<PublishContentTask> s) : base(contentManager, s)
        {
        }

        public override string Name => nameof(PublishContentTask);
        public override LocalizedString Category => S["Content"];
        public override LocalizedString Description => S["Publish the content item."];

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(S["Published"]);
        }

        public override async Task<IEnumerable<string>> ExecuteAsync(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            var content = await GetContentAsync(workflowContext);
            await ContentManager.PublishAsync(content.ContentItem);
            return new[] { "Published" };
        }
    }
}