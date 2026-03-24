using OrchardCore.Localization.Data;

namespace OrchardCore.AdminMenu.Services;

public class AdminMenuDataLocalizationProvider : ILocalizationDataProvider
{
    private readonly IAdminMenuService _adminMenuService;

    public AdminMenuDataLocalizationProvider(IAdminMenuService adminMenuService)
    {
        _adminMenuService = adminMenuService;
    }

    public async Task<IEnumerable<DataLocalizedString>> GetDescriptorsAsync()
    {
        var adminMenuList = await _adminMenuService.GetAdminMenuListAsync();

        return adminMenuList.AdminMenu.Select(item => new DataLocalizedString(DataLocalizationContext.AdminMenu(), item.Name, string.Empty));
    }
}
