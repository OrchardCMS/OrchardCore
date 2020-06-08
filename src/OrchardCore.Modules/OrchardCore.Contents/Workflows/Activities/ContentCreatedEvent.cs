using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Contents.Workflows.Activities
{
    public class ContentCreatedEvent : ContentEvent
    {
        public ContentCreatedEvent(IContentManager contentManager, IWorkflowScriptEvaluator scriptEvaluator, IStringLocalizer<ContentCreatedEvent> localizer) : base(contentManager, scriptEvaluator, localizer)
        {
        }

        public override string Name => nameof(ContentCreatedEvent);

        public override LocalizedString DisplayText => S["Content Created Event"];
    }
}
