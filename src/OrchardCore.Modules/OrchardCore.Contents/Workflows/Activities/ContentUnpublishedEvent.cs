using System.Text.Json;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Contents.Workflows.Activities
{
    public class ContentUnpublishedEvent : ContentEvent
    {
        public ContentUnpublishedEvent(
            IContentManager contentManager,
            IWorkflowScriptEvaluator scriptEvaluator,
            IOptions<JsonSerializerOptions> jsonSerializerOptions,
            IStringLocalizer<ContentUnpublishedEvent> localizer)
            : base(contentManager, scriptEvaluator, localizer, jsonSerializerOptions)
        {
        }

        public override string Name => nameof(ContentUnpublishedEvent);

        public override LocalizedString DisplayText => S["Content Unpublished Event"];
    }
}
