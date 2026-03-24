using OrchardCore.AdminMenu.Services;
using OrchardCore.Lists.AdminNodes;
using OrchardCore.Localization.Data;

namespace OrchardCore.Lists.Services;

public class ListsAdminNodeDataLocalizationProvider : AdminNodeDataLocalizationProvider
{
    public ListsAdminNodeDataLocalizationProvider(IAdminMenuService adminMenuService) : base(adminMenuService)
    {
    }

    public override async Task<IEnumerable<DataLocalizedString>> GetDescriptorsAsync()
    {
        var adminMenuList = await GetAdminMenuListAsync();

        return adminMenuList.SelectMany(m => m.MenuItems.OfType<ListsAdminNode>()
            .Select(n => new DataLocalizedString(DataLocalizationContext.AdminMenus(m.Name), n.ContentType, string.Empty))
        );
    }
}
