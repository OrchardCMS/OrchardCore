using OrchardCore.AdminMenu;
using OrchardCore.AdminMenu.Services;
using OrchardCore.Contents.AdminNodes;
using OrchardCore.Localization.Data;

namespace OrchardCore.Contents.Services;

public class ContentTypesAdminNodeDataLocalizationProvider : AdminNodeDataLocalizationProvider
{
    public ContentTypesAdminNodeDataLocalizationProvider(IAdminMenuAccessor adminMenuRetrieval) : base(adminMenuRetrieval)
    {
    }

    public override async Task<IEnumerable<DataLocalizedString>> GetDescriptorsAsync()
    {
        var adminMenuList = await GetAdminMenusAsync();

        return adminMenuList.SelectMany(m => m.MenuItems.OfType<ContentTypesAdminNode>()
            .SelectMany(n => n.ContentTypes)
            .Select(e => new DataLocalizedString(OrchardCore.AdminMenu.DataLocalizationContext.AdminMenu(m.Name), e.ContentTypeDisplayName, string.Empty))
        );
    }
}
