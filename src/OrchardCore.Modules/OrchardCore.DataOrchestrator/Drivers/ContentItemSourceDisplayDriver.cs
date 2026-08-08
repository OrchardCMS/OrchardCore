using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DataOrchestrator.Activities;
using OrchardCore.DataOrchestrator.Display;
using OrchardCore.DataOrchestrator.ViewModels;

namespace OrchardCore.DataOrchestrator.Drivers;

public sealed class ContentItemSourceDisplayDriver : EtlActivityDisplayDriver<ContentItemSource, ContentItemSourceViewModel>
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public ContentItemSourceDisplayDriver(IContentDefinitionManager contentDefinitionManager)
    {
        _contentDefinitionManager = contentDefinitionManager;
    }

    protected override async ValueTask EditActivityAsync(ContentItemSource activity, ContentItemSourceViewModel model)
    {
        model.AvailableContentTypes = (await _contentDefinitionManager.ListTypeDefinitionsAsync())
            .Select(x => new SelectListItem { Text = x.DisplayName, Value = x.Name })
            .ToList();
        model.ContentType = activity.ContentType;
        model.VersionScope = activity.VersionScope;
        model.Owner = activity.Owner;
        model.CreatedUtcFrom = activity.CreatedUtcFrom;
        model.CreatedUtcTo = activity.CreatedUtcTo;
    }

    protected override void UpdateActivity(ContentItemSourceViewModel model, ContentItemSource activity)
    {
        activity.ContentType = model.ContentType;
        activity.VersionScope = model.VersionScope;
        activity.Owner = model.Owner;
        activity.CreatedUtcFrom = model.CreatedUtcFrom;
        activity.CreatedUtcTo = model.CreatedUtcTo;
    }
}
