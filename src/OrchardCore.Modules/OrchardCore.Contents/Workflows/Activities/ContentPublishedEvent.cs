using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.Localization;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Contents.Workflows.Activities;

public class ContentPublishedEvent : ContentEvent, IActivityDescription
{
    public ContentPublishedEvent(
        IContentManager contentManager,
        IWorkflowScriptEvaluator scriptEvaluator,
        IStringLocalizerFactory localizerFactory,
        IStringLocalizer<ContentPublishedEvent> localizer)
        : base(contentManager, scriptEvaluator, localizerFactory, localizer)
    {
    }

    public override string Name => nameof(ContentPublishedEvent);

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<ContentPublishedEvent>("Content Published Event");

    public override LocalizedString DisplayText => LocalizerFactory.Localize(DisplayName);
}
