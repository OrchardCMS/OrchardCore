using OrchardCore.AdminMenu.AdminNodes;
using OrchardCore.Localization.Data;

namespace OrchardCore.AdminMenu.Services;

public class LinkAdminNodeDataLocalizationProvider : AdminNodeDataLocalizationProvider
{
    public LinkAdminNodeDataLocalizationProvider(IAdminMenuAccessor adminMenuAccessor) : base(adminMenuAccessor)
    {
    }

    public override async Task<IEnumerable<DataLocalizedString>> GetDescriptorsAsync()
    {
        var adminMenuList = await GetAdminMenusAsync();

        return adminMenuList.SelectMany(m =>
        {
            var context = DataLocalizationContext.AdminMenu(m.Name);

            return m.MenuItems.OfType<LinkAdminNode>()
                .Select(n => new DataLocalizedString(context, n.LinkText, string.Empty));
        });
    }
}
