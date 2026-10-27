using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.Localization;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Contents.Workflows.Activities;

public class ContentDraftSavedEvent : ContentEvent, IActivityDescription
{
    public ContentDraftSavedEvent(
        IContentManager contentManager,
        IWorkflowScriptEvaluator scriptEvaluator,
        IStringLocalizerFactory localizerFactory,
        IStringLocalizer<ContentDraftSavedEvent> localizer)
        : base(contentManager, scriptEvaluator, localizerFactory, localizer)
    {
    }

    public override string Name => nameof(ContentDraftSavedEvent);

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<ContentDraftSavedEvent>("Content Save Draft Event");

    public override LocalizedString DisplayText => LocalizerFactory.Localize(DisplayName);
}
