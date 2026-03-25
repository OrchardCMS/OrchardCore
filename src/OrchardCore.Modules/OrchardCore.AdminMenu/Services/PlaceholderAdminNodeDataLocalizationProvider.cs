using OrchardCore.AdminMenu.AdminNodes;
using OrchardCore.Localization.Data;

namespace OrchardCore.AdminMenu.Services;

public class PlaceholderAdminNodeDataLocalizationProvider : AdminNodeDataLocalizationProvider
{
    public PlaceholderAdminNodeDataLocalizationProvider(IAdminMenuAccessor adminMenuRetrieval) : base(adminMenuRetrieval)
    {
    }

    public override async Task<IEnumerable<DataLocalizedString>> GetDescriptorsAsync()
    {
        var adminMenuList = await GetAdminMenusAsync();

        return adminMenuList.SelectMany(m =>
        {
            var context = string.Concat(OrchardCoreConstants.DataLocalizationContext.AdminMenu, Constants.ContextSeparator, m.Name);

            return m.MenuItems.OfType<PlaceholderAdminNode>()
                .Select(n => new DataLocalizedString(context, n.LinkText, string.Empty));
        });
    }
}
