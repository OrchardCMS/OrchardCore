using OrchardCore.AdminMenu;
using OrchardCore.AdminMenu.Services;
using OrchardCore.Lists.AdminNodes;
using OrchardCore.Localization.Data;

namespace OrchardCore.Lists.Services;

public class ListsAdminNodeDataLocalizationProvider : AdminNodeDataLocalizationProvider
{
    public ListsAdminNodeDataLocalizationProvider(IAdminMenuAccessor adminMenuRetrieval) : base(adminMenuRetrieval)
    {
    }

    public override async Task<IEnumerable<DataLocalizedString>> GetDescriptorsAsync()
    {
        var adminMenuList = await GetAdminMenusAsync();

        return adminMenuList.SelectMany(m => m.MenuItems.OfType<ListsAdminNode>()
            .Select(n => new DataLocalizedString(OrchardCore.AdminMenu.DataLocalizationContext.AdminMenu(m.Name), n.ContentType, string.Empty))
        );
    }
}
