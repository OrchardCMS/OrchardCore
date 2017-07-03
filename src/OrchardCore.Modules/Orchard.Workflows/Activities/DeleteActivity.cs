using System.Collections.Generic;
using Microsoft.Extensions.Localization;
using Orchard.ContentManagement;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;

namespace Orchard.Workflows.Activities
{
    public class DeleteActivity : TaskActivity
    {
        private readonly IContentManager _contentManager;

        private readonly IStringLocalizer S;

        public DeleteActivity(IContentManager contentManager,
            IStringLocalizer<DeleteActivity> s)
        {
            _contentManager = contentManager;

            S = s;
        }

        public override bool CanExecute(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            return true;
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            return new[] { S["Deleted"] };
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            _contentManager.RemoveAsync(workflowContext.Content.ContentItem).GetAwaiter().GetResult();
            yield return S["Deleted"];
        }

        public override string Name
        {
            get { return "Delete"; }
        }

        public override LocalizedString Category
        {
            get { return S["Content Items"]; }
        }

        public override LocalizedString Description
        {
            get { return S["Delete the content item."]; }
        }
    }
}