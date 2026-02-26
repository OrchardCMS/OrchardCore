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

        return adminMenuList.SelectMany(m =>
        {
            var context = string.Concat(OrchardCoreConstants.DataLocalizationContext.AdminMenu, Constants.ContextSeparator, m.Name);

            return m.MenuItems.OfType<LinkAdminNode>()
                .Select(n => new DataLocalizedString(context, n.LinkText, string.Empty));
        });
    }
}
