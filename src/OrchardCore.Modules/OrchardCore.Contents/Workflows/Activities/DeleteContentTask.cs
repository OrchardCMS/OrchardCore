using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Contents.Workflows.Activities
{
    public class DeleteContentTask : ContentTask
    {
        private readonly IContentManager _contentManager;

        public DeleteContentTask(IContentManager contentManager, IStringLocalizer<DeleteContentTask> s) : base(s)
        {
            _contentManager = contentManager;
        }

        public override string Name => nameof(DeleteContentTask);
        public override LocalizedString Category => S["Content Items"];
        public override LocalizedString Description => S["Delete the content item."];

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            yield return S["Deleted"];
        }

        public override async Task<IEnumerable<LocalizedString>> ExecuteAsync(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            var content = GetContent(workflowContext);
            await _contentManager.RemoveAsync(content.ContentItem);
            return new[] { S["Deleted"] };
        }
    }
}