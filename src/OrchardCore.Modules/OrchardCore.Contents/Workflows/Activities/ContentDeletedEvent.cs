using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.Localization;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Contents.Workflows.Activities;

public class ContentDeletedEvent : ContentEvent, IActivityDescription
{
    public ContentDeletedEvent(
        IContentManager contentManager,
        IWorkflowScriptEvaluator scriptEvaluator,
        IStringLocalizerFactory localizerFactory,
        IStringLocalizer<ContentDeletedEvent> localizer)
        : base(contentManager, scriptEvaluator, localizerFactory, localizer)
    {
    }

    public override string Name => nameof(ContentDeletedEvent);

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<ContentDeletedEvent>("Content Deleted Event");

    public override LocalizedString DisplayText => LocalizerFactory.Localize(DisplayName);
}
