using OrchardCore.AdminMenu.Services;
using OrchardCore.Contents.AdminNodes;
using OrchardCore.Localization.Data;

namespace OrchardCore.Contents.Services;

public class ContentTypesAdminNodeDataLocalizationProvider : AdminNodeDataLocalizationProvider
{
    public ContentTypesAdminNodeDataLocalizationProvider(IAdminMenuService adminMenuService) : base(adminMenuService)
    {
    }

    public override async Task<IEnumerable<DataLocalizedString>> GetDescriptorsAsync()
    {
        var adminMenuList = await GetAdminMenuListAsync();

        return adminMenuList.SelectMany(m => m.MenuItems.OfType<ContentTypesAdminNode>()
            .SelectMany(n => n.ContentTypes)
            .Select(e => new DataLocalizedString(DataLocalizationContext.AdminMenus(m.Name), e.ContentTypeDisplayName, string.Empty))
        );
    }
}
