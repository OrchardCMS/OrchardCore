using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Contents.Workflows.Activities
{
    public abstract class ContentEvent : ContentActivity, IEvent
    {
        protected ContentEvent(IContentManager contentManager, IStringLocalizer localizer) : base(contentManager, localizer)
        {
        }

        public virtual bool CanStartWorkflow => true;

        public override ActivityExecutionResult Execute(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            return Halt();
        }

        public override ActivityExecutionResult Resume(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes("Done");
        }
    }
}