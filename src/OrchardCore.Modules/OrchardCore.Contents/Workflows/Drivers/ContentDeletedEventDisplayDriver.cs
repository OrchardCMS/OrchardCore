using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Contents.Workflows.Activities;
using OrchardCore.Contents.Workflows.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Localization;

namespace OrchardCore.Contents.Workflows.Drivers;

public sealed class ContentDeletedEventDisplayDriver : ContentEventDisplayDriver<ContentDeletedEvent, ContentDeletedEventViewModel>
{
    private readonly IStringLocalizerFactory _localizerFactory;

    internal readonly IStringLocalizer S;

    public ContentDeletedEventDisplayDriver(
        IContentDefinitionManager contentDefinitionManager,
        IStringLocalizer<ContentDeletedEventDisplayDriver> stringLocalizer,
        IStringLocalizerFactory localizerFactory)
        : base(contentDefinitionManager)
    {
        _localizerFactory = localizerFactory;
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(ContentDeletedEvent activity, BuildEditorContext context)
    {
        return Initialize<ContentDeletedEventViewModel>("ContentDeletedEvent_Fields_Edit", model => EditActivityAsync(activity, model))
            .Location(l => l.Zone("Content", "1").Card(_localizerFactory.Localize(CommonLocalizationSources.ContentTypes), "10"));
    }
}
