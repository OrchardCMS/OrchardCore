using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.Localization;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Contents.Workflows.Activities;

public class ContentUpdatedEvent : ContentEvent, IActivityDescription
{
    public ContentUpdatedEvent(
        IContentManager contentManager,
        IWorkflowScriptEvaluator scriptEvaluator,
        IStringLocalizerFactory localizerFactory,
        IStringLocalizer<ContentUpdatedEvent> localizer)
        : base(contentManager, scriptEvaluator, localizerFactory, localizer)
    {
    }

    public override string Name => nameof(ContentUpdatedEvent);

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<ContentUpdatedEvent>("Content Updated Event");

    public override LocalizedString DisplayText => LocalizerFactory.Localize(DisplayName);
}
