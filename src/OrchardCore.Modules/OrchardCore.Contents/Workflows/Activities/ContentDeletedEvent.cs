using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Contents.Workflows.Activities
{
    public class ContentDeletedEvent : ContentEvent
    {
        public ContentDeletedEvent(IContentManager contentManager, IWorkflowScriptEvaluator scriptEvaluator, IStringLocalizer<ContentDeletedEvent> localizer) : base(contentManager, scriptEvaluator, localizer)
        {
        }

        public override string Name => nameof(ContentDeletedEvent);

        public override LocalizedString DisplayText => S["Content Deleted Event"];
    }
}
