using System.Text.Json;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Contents.Workflows.Activities
{
    public class ContentDeletedEvent : ContentEvent
    {
        public ContentDeletedEvent(
            IContentManager contentManager,
            IWorkflowScriptEvaluator scriptEvaluator,
            IOptions<JsonSerializerOptions> jsonSerializerOptions,
            IStringLocalizer<ContentDeletedEvent> localizer)
            : base(contentManager, scriptEvaluator, localizer, jsonSerializerOptions)
        {
        }

        public override string Name => nameof(ContentDeletedEvent);

        public override LocalizedString DisplayText => S["Content Deleted Event"];
    }
}
