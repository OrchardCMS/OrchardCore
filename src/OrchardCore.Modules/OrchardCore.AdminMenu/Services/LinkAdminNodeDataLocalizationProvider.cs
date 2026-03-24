using OrchardCore.AdminMenu.AdminNodes;
using OrchardCore.Localization.Data;

namespace OrchardCore.AdminMenu.Services;

public class ListsAdminNodeDataLocalizationProvider : AdminNodeDataLocalizationProvider
{
    public ListsAdminNodeDataLocalizationProvider(IAdminMenuService adminMenuService) : base(adminMenuService)
    {
    }

    public override async Task<IEnumerable<DataLocalizedString>> GetDescriptorsAsync()
    {
        var adminMenuList = await GetAdminMenuListAsync();

        return adminMenuList.SelectMany(m => m.MenuItems.OfType<LinkAdminNode>()
            .Select(n => new DataLocalizedString(DataLocalizationContext.AdminMenus(m.Name), n.LinkText, string.Empty))
        );
    }
}
