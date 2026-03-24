using OrchardCore.AdminMenu.AdminNodes;
using OrchardCore.Localization.Data;

namespace OrchardCore.AdminMenu.Services;

public class PlaceholderAdminNodeDataLocalizationProvider : AdminNodeDataLocalizationProvider
{
    public PlaceholderAdminNodeDataLocalizationProvider(IAdminMenuService adminMenuService) : base(adminMenuService)
    {
    }

    public override async Task<IEnumerable<DataLocalizedString>> GetDescriptorsAsync()
    {
        var adminMenuList = await GetAdminMenuListAsync();

        return adminMenuList.SelectMany(m => m.MenuItems.OfType<PlaceholderAdminNode>()
            .Select(n => new DataLocalizedString(DataLocalizationContext.AdminMenu(m.Name), n.LinkText, string.Empty))
        );
    }
}
