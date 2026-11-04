using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.Localization;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Contents.Workflows.Activities;

public class ContentVersionedEvent : ContentEvent, IActivityDescription
{
    public ContentVersionedEvent(IContentManager contentManager,
        IWorkflowScriptEvaluator scriptEvaluator,
        IStringLocalizerFactory localizerFactory,
        IStringLocalizer<ContentVersionedEvent> localizer)
        : base(contentManager, scriptEvaluator, localizerFactory, localizer)
    {
    }

    public override string Name => nameof(ContentVersionedEvent);

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<ContentVersionedEvent>("Content Versioned Event");

    public override LocalizedString DisplayText => LocalizerFactory.Localize(DisplayName);
}
