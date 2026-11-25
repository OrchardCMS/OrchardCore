using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Contents.Workflows.Activities;
using OrchardCore.Contents.Workflows.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Contents.Workflows.Drivers;

public sealed class ContentDraftSavedEventDisplayDriver : ContentEventDisplayDriver<ContentDraftSavedEvent, ContentDraftSavedEventViewModel>
{
    internal readonly IStringLocalizer S;

    public ContentDraftSavedEventDisplayDriver(
        IContentDefinitionManager contentDefinitionManager,
        IStringLocalizer<ContentDraftSavedEventDisplayDriver> stringLocalizer)
        : base(contentDefinitionManager)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(ContentDraftSavedEvent activity, BuildEditorContext context)
    {
        return Initialize<ContentDraftSavedEventViewModel>("ContentDraftSavedEvent_Fields_Edit", model => EditActivityAsync(activity, model))
            .Location(l => l.Zone("Content", "1").Card(S["Content types"], "10"));
    }
}
