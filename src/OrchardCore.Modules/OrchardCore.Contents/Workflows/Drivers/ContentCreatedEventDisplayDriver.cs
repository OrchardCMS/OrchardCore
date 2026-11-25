using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Contents.Workflows.Activities;
using OrchardCore.Contents.Workflows.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Contents.Workflows.Drivers;

public sealed class ContentCreatedEventDisplayDriver : ContentEventDisplayDriver<ContentCreatedEvent, ContentCreatedEventViewModel>
{
    internal readonly IStringLocalizer S;

    public ContentCreatedEventDisplayDriver(
        IContentDefinitionManager contentDefinitionManager,
        IStringLocalizer<ContentCreatedEventDisplayDriver> stringLocalizer)
        : base(contentDefinitionManager)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(ContentCreatedEvent activity, BuildEditorContext context)
    {
        return Initialize<ContentCreatedEventViewModel>("ContentCreatedEvent_Fields_Edit", model => EditActivityAsync(activity, model))
            .Location(l => l.Zone("Content", "1").Card(S["Content types"], "10"));
    }
}
