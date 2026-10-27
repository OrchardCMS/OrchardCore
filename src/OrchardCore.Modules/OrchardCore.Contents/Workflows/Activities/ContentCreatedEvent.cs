using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.Localization;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Contents.Workflows.Activities;

public class ContentCreatedEvent : ContentEvent, IActivityDescription
{
    public ContentCreatedEvent(
        IContentManager contentManager,
        IWorkflowScriptEvaluator scriptEvaluator,
        IStringLocalizerFactory localizerFactory,
        IStringLocalizer<ContentCreatedEvent> localizer)
        : base(contentManager, scriptEvaluator, localizerFactory, localizer)
    {
    }

    public override string Name => nameof(ContentCreatedEvent);

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<ContentCreatedEvent>("Content Created Event");

    public override LocalizedString DisplayText => LocalizerFactory.Localize(DisplayName);
}
