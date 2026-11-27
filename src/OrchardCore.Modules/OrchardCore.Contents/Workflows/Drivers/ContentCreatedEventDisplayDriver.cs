using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Contents.Workflows.Activities;
using OrchardCore.Contents.Workflows.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Localization;

namespace OrchardCore.Contents.Workflows.Drivers;

public sealed class ContentCreatedEventDisplayDriver : ContentEventDisplayDriver<ContentCreatedEvent, ContentCreatedEventViewModel>
{
    private readonly IStringLocalizerFactory _localizerFactory;

    internal readonly IStringLocalizer S;

    public ContentCreatedEventDisplayDriver(
        IContentDefinitionManager contentDefinitionManager,
        IStringLocalizer<ContentCreatedEventDisplayDriver> stringLocalizer,
        IStringLocalizerFactory localizerFactory)
        : base(contentDefinitionManager)
    {
        _localizerFactory = localizerFactory;
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(ContentCreatedEvent activity, BuildEditorContext context)
    {
        return Initialize<ContentCreatedEventViewModel>("ContentCreatedEvent_Fields_Edit", model => EditActivityAsync(activity, model))
            .Location(l => l.Zone("Content", "1").Card(_localizerFactory.Localize(CommonLocalizationSources.ContentTypes), "10"));
    }
}
