using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.Workflows.Abstractions.Models;
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
        public override LocalizedString Description => S["Delete a content item."];

        public string ContentParameterName
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(S["Deleted"]);
        }

        public override async Task<IEnumerable<string>> ExecuteAsync(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            var content = GetContent(workflowContext);
            await _contentManager.RemoveAsync(content.ContentItem);
            return new[] { "Deleted" };
        }
    }
}