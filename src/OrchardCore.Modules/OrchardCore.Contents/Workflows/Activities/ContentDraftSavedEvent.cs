using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Contents.Workflows.Activities
{
    public class ContentDraftSavedEvent : ContentEvent
    {
        public ContentDraftSavedEvent(IContentManager contentManager, IWorkflowScriptEvaluator scriptEvaluator, IStringLocalizer<ContentDraftSavedEvent> localizer) : base(contentManager, scriptEvaluator, localizer)
        {
        }

        public override string Name => nameof(ContentDraftSavedEvent);

        public override LocalizedString DisplayText => S["Content Save Draft Event"];
    }
}
