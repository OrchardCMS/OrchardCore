using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Contents.Workflows.Activities;
using OrchardCore.Contents.Workflows.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Contents.Workflows.Drivers;

public sealed class ContentVersionedEventDisplayDriver : ContentEventDisplayDriver<ContentVersionedEvent, ContentVersionedEventViewModel>
{
    internal readonly IStringLocalizer S;

    public ContentVersionedEventDisplayDriver(
        IContentDefinitionManager contentDefinitionManager,
        IStringLocalizer<ContentVersionedEventDisplayDriver> stringLocalizer)
        : base(contentDefinitionManager)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(ContentVersionedEvent activity, BuildEditorContext context)
    {
        return Initialize<ContentVersionedEventViewModel>("ContentVersionedEvent_Fields_Edit", model => EditActivityAsync(activity, model))
            .Location(l => l.Zone("Content", "1").Card(S["Content types"], "10"));
    }
}
