using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Contents.Workflows.Activities
{
    public class ContentUnpublishedEvent : ContentEvent
    {
        public ContentUnpublishedEvent(IContentManager contentManager, IWorkflowScriptEvaluator scriptEvaluator, IStringLocalizer<ContentUnpublishedEvent> localizer) : base(contentManager, scriptEvaluator, localizer)
        {
        }

        public override string Name => nameof(ContentUnpublishedEvent);

        public override LocalizedString DisplayText => S["Content Unpublished Event"];
    }
}
