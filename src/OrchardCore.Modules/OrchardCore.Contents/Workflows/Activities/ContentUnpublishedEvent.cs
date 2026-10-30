using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.Localization;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Contents.Workflows.Activities;

public class ContentUnpublishedEvent : ContentEvent, IActivityDescription
{
    public ContentUnpublishedEvent(
        IContentManager contentManager,
        IWorkflowScriptEvaluator scriptEvaluator,
        IStringLocalizerFactory localizerFactory,
        IStringLocalizer<ContentUnpublishedEvent> localizer)
        : base(contentManager, scriptEvaluator, localizerFactory, localizer)
    {
    }

    public override string Name => nameof(ContentUnpublishedEvent);

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<ContentUnpublishedEvent>("Content Unpublished Event");

    public override LocalizedString DisplayText => LocalizerFactory.Localize(DisplayName);
}
