using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Contents.Workflows.Activities
{
    public class ContentUpdatedEvent : ContentEvent
    {
        public ContentUpdatedEvent(IContentManager contentManager, IWorkflowScriptEvaluator scriptEvaluator, IStringLocalizer<ContentUpdatedEvent> localizer) : base(contentManager, scriptEvaluator, localizer)
        {
        }

        public override string Name => nameof(ContentUpdatedEvent);

        public override LocalizedString DisplayText => S["Content Updated Event"];
    }
}
