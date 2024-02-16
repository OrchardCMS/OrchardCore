using System.Text.Json;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Contents.Workflows.Activities
{
    public class ContentPublishedEvent : ContentEvent
    {
        public ContentPublishedEvent(
            IContentManager contentManager,
            IWorkflowScriptEvaluator scriptEvaluator,
            IOptions<JsonSerializerOptions> jsonSerializerOptions,
            IStringLocalizer<ContentPublishedEvent> localizer)
            : base(contentManager, scriptEvaluator, localizer, jsonSerializerOptions)
        {
        }

        public override string Name => nameof(ContentPublishedEvent);

        public override LocalizedString DisplayText => S["Content Published Event"];
    }
}
