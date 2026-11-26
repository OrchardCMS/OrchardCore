using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Contents.Workflows.Activities;
using OrchardCore.Contents.Workflows.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Contents.Workflows.Drivers;

public sealed class ContentUnpublishedEventDisplayDriver : ContentEventDisplayDriver<ContentUnpublishedEvent, ContentUnpublishedEventViewModel>
{
    internal readonly IStringLocalizer S;

    public ContentUnpublishedEventDisplayDriver(
        IContentDefinitionManager contentDefinitionManager,
        IStringLocalizer<ContentUnpublishedEventDisplayDriver> stringLocalizer)
        : base(contentDefinitionManager)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(ContentUnpublishedEvent activity, BuildEditorContext context)
    {
        return Initialize<ContentUnpublishedEventViewModel>("ContentUnpublishedEvent_Fields_Edit", model => EditActivityAsync(activity, model))
            .Location(l => l.Zone("Content", "1").Card(S["Content types"], "10"));
    }
}
