using OrchardCore.AdminMenu.AdminNodes;
using OrchardCore.Localization.Data;

namespace OrchardCore.AdminMenu.Services;

public class ListsAdminNodeDataLocalizationProvider : AdminNodeDataLocalizationProvider
{
    public ListsAdminNodeDataLocalizationProvider(IAdminMenuAccessor adminMenuRetrieval) : base(adminMenuRetrieval)
    {
    }

    public override async Task<IEnumerable<DataLocalizedString>> GetDescriptorsAsync()
    {
        var adminMenuList = await GetAdminMenusAsync();

        return adminMenuList.SelectMany(m => m.MenuItems.OfType<LinkAdminNode>()
            .Select(n => new DataLocalizedString(DataLocalizationContext.AdminMenu(m.Name), n.LinkText, string.Empty))
        );
    }
}
