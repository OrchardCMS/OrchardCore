using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Contents.Workflows.Activities;
using OrchardCore.Contents.Workflows.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Contents.Workflows.Drivers;

public sealed class ContentUpdatedEventDisplayDriver : ContentEventDisplayDriver<ContentUpdatedEvent, ContentUpdatedEventViewModel>
{
    internal readonly IStringLocalizer S;

    public ContentUpdatedEventDisplayDriver(
        IContentDefinitionManager contentDefinitionManager,
        IStringLocalizer<ContentUpdatedEventDisplayDriver> stringLocalizer)
        : base(contentDefinitionManager)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(ContentUpdatedEvent activity, BuildEditorContext context)
    {
        return Initialize<ContentUpdatedEventViewModel>("ContentUpdatedEvent_Fields_Edit", model => EditActivityAsync(activity, model))
            .Location(l => l.Zone("Content", "1").Card(S["Content types"], "10"));
    }
}
