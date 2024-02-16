using System.Text.Json;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Contents.Workflows.Activities
{
    public class ContentVersionedEvent : ContentEvent
    {
        public ContentVersionedEvent(IContentManager contentManager,
            IWorkflowScriptEvaluator scriptEvaluator,
            IOptions<JsonSerializerOptions> jsonSerializerOptions,
            IStringLocalizer<ContentVersionedEvent> localizer)
            : base(contentManager, scriptEvaluator, localizer, jsonSerializerOptions)
        {
        }

        public override string Name => nameof(ContentVersionedEvent);

        public override LocalizedString DisplayText => S["Content Versioned Event"];
    }
}
