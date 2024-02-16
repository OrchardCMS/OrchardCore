using System.Text.Json;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Contents.Workflows.Activities
{
    public class ContentUpdatedEvent : ContentEvent
    {
        public ContentUpdatedEvent(
            IContentManager contentManager,
            IWorkflowScriptEvaluator scriptEvaluator,
            IOptions<JsonSerializerOptions> jsonSerializerOptions,
            IStringLocalizer<ContentUpdatedEvent> localizer)
            : base(contentManager, scriptEvaluator, localizer, jsonSerializerOptions)
        {
        }

        public override string Name => nameof(ContentUpdatedEvent);

        public override LocalizedString DisplayText => S["Content Updated Event"];
    }
}
