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

        return adminMenuList.SelectMany(m =>
        {
            var context = string.Concat(Context, Constants.ContextSeparator, m.Name);

            return m.MenuItems.OfType<PlaceholderAdminNode>()
                .Select(n => new DataLocalizedString(context, n.LinkText, string.Empty));
        });
    }
}
