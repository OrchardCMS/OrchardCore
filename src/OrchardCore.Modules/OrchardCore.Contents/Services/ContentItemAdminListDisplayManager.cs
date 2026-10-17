using OrchardCore.Admin;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;

namespace OrchardCore.Contents.Services;

public sealed class ContentItemAdminListDisplayManager : IAdminListItemDisplayManager<ContentItem>
{
    private readonly IContentItemDisplayManager _displayManager;

    public ContentItemAdminListDisplayManager(IContentItemDisplayManager displayManager)
    {
        _displayManager = displayManager;
    }

    public Task<IShape> BuildDisplayAsync(ContentItem model, IUpdateModel updater, string displayType)
        => _displayManager.BuildDisplayAsync(model, updater, displayType);
}
