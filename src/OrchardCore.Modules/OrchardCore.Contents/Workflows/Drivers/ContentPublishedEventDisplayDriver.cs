using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Contents.Workflows.Activities;
using OrchardCore.Contents.Workflows.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Localization;

namespace OrchardCore.Contents.Workflows.Drivers;

public sealed class ContentPublishedEventDisplayDriver : ContentEventDisplayDriver<ContentPublishedEvent, ContentPublishedEventViewModel>
{
    private readonly IStringLocalizerFactory _localizerFactory;

    internal readonly IStringLocalizer S;

    public ContentPublishedEventDisplayDriver(
        IContentDefinitionManager contentDefinitionManager,
        IStringLocalizer<ContentPublishedEventDisplayDriver> stringLocalizer,
        IStringLocalizerFactory localizerFactory)
        : base(contentDefinitionManager)
    {
        _localizerFactory = localizerFactory;
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(ContentPublishedEvent activity, BuildEditorContext context)
    {
        return Initialize<ContentPublishedEventViewModel>("ContentPublishedEvent_Fields_Edit", model => EditActivityAsync(activity, model))
            .Location(l => l.Zone("Content", "1").Card(_localizerFactory.Localize(CommonLocalizationSources.ContentTypes), "10"));
    }
}
