using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Contents.Workflows.Activities
{
    public abstract class ContentTask : ContentActivity, ITask
    {
        protected ContentTask(IContentManager contentManager, IWorkflowScriptEvaluator scriptEvaluator, IStringLocalizer localizer)
            : base(contentManager, scriptEvaluator, localizer)
        {
        }
    }
}
