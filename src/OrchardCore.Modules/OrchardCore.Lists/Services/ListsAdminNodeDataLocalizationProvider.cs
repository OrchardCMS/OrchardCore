using OrchardCore.AdminMenu.Services;
using OrchardCore.Lists.AdminNodes;
using OrchardCore.Localization.Data;

namespace OrchardCore.Lists.Services;

public class ListsAdminNodeDataLocalizationProvider : AdminNodeDataLocalizationProvider
{
    public ListsAdminNodeDataLocalizationProvider(IAdminMenuRetrieval adminMenuRetrieval) : base(adminMenuRetrieval)
    {
    }

    public override async Task<IEnumerable<DataLocalizedString>> GetDescriptorsAsync()
    {
        var adminMenuList = await GetAdminMenuAsync();

        return adminMenuList.SelectMany(m =>
        {
            var context = string.Concat(OrchardCoreConstants.DataLocalizationContext.AdminMenu, Constants.ContextSeparator, m.Name);

            return m.MenuItems.OfType<ListsAdminNode>()
                .Select(n => new DataLocalizedString(context, n.ContentType, string.Empty));
        });
    }
}
