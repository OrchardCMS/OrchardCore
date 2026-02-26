using OrchardCore.Localization.Data;

namespace OrchardCore.AdminMenu.Services;

public class AdminMenuDataLocalizationProvider : ILocalizationDataProvider
{
    private readonly IAdminMenuService _adminMenuService;

    internal static readonly string Context = "Admin Menus";

    public AdminMenuDataLocalizationProvider(IAdminMenuService adminMenuService)
    {
        _adminMenuService = adminMenuService;
    }

    public async Task<IEnumerable<DataLocalizedString>> GetDescriptorsAsync()
    {
        var adminMenuList = await _adminMenuService.GetAdminMenuListAsync();

        return adminMenuList.AdminMenu.Select(item => new DataLocalizedString(Context, item.Name, string.Empty));
    }
}
