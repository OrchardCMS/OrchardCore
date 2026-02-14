using OrchardCore.AdminMenu.AdminNodes;
using OrchardCore.Localization.Data;
using OrchardCore.Navigation;

namespace OrchardCore.AdminMenu.Services;

public class AdminMenuItemDataLocalizationProvider : ILocalizationDataProvider
{
    private readonly IAdminMenuService _adminMenuService;

    private static readonly string _adminMenuNodeContext = "Admin Menu Items";

    public AdminMenuItemDataLocalizationProvider(IAdminMenuService adminMenuService)
    {
        _adminMenuService = adminMenuService;
    }

    public async Task<IEnumerable<DataLocalizedString>> GetDescriptorsAsync()
    {
        var adminMenuList = await _adminMenuService.GetAdminMenuListAsync();

        var localizedStrings = new List<DataLocalizedString>();

        var adminNodes = adminMenuList.AdminMenu.SelectMany(item => item.MenuItems);

        foreach (var adminNode in adminNodes)
        {
            GetAdminMenuNodes(adminNode, localizedStrings);
        }

        return localizedStrings;
    }

    private static void GetAdminMenuNodes(MenuItem menuItem, List<DataLocalizedString> localizedStrings)
    {
        if (menuItem.Items.Count == 0)
        {
            return;
        }

        foreach (var item in menuItem.Items)
        {
            if (item is LinkAdminNode)
            {
                var context = string.Concat(_adminMenuNodeContext, Constants.ContextSeparator, item.GetType().Name);
                localizedStrings.Add(new DataLocalizedString(context, ((LinkAdminNode)item).LinkText, string.Empty));
            }

            GetAdminMenuNodes(item, localizedStrings);
        }
    }
}
