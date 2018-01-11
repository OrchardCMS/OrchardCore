using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.Workflows.Activities;

namespace OrchardCore.Contents.Workflows.Activities
{
    public abstract class ContentEvent : ContentActivity, IEvent
    {
        protected ContentEvent(IContentManager contentManager, IStringLocalizer localizer) : base(contentManager, localizer)
        {
        }

        public virtual bool CanStartWorkflow => true;
    }
}