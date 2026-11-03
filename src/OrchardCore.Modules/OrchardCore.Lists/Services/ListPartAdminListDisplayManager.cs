using OrchardCore.Admin;
using OrchardCore.ContentManagement.Display;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Zones;
using OrchardCore.Lists.Models;

namespace OrchardCore.Lists.Services;

public sealed class ListPartAdminListDisplayManager : IAdminListItemDisplayManager<ListPartAdminListEntry>
{
    private readonly IContentItemDisplayManager _displayManager;
    private readonly IShapeFactory _shapeFactory;

    public ListPartAdminListDisplayManager(IContentItemDisplayManager displayManager, IShapeFactory shapeFactory)
    {
        _displayManager = displayManager;
        _shapeFactory = shapeFactory;
    }

    public async Task<IShape> BuildDisplayAsync(ListPartAdminListEntry model, IUpdateModel updater, string displayType)
    {
        var row = await _displayManager.BuildDisplayAsync(model.ContentItem, model.Updater ?? updater, displayType, model.GroupId);
        row.Properties.Remove("Checkbox");
        row.Properties.Remove("Select");

        if (model.EnableOrdering)
        {
            await ((IZoneHolding)row).Zones["Selectors"].AddAsync(await _shapeFactory.CreateAsync("ListPart_Sortable"));
        }

        return row;
    }
}
