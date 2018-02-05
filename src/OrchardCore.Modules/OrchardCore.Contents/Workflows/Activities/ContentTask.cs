using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.Workflows.Activities;

namespace OrchardCore.Contents.Workflows.Activities
{
    public abstract class ContentTask : ContentActivity, ITask
    {
        protected ContentTask(IContentManager contentManager, IStringLocalizer localizer) : base(contentManager, localizer)
        {
        }
    }
}