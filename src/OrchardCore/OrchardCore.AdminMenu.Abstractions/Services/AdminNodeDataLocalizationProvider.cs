using OrchardCore.Localization.Data;

namespace OrchardCore.AdminMenu.Services;

public abstract class AdminNodeDataLocalizationProvider : ILocalizationDataProvider
{
    private readonly IAdminMenuService _adminMenuService;

    public static readonly string Context = "Admin Menu Items";

    public AdminNodeDataLocalizationProvider(IAdminMenuService adminMenuService)
    {
        _adminMenuService = adminMenuService;
    }

    public abstract Task<IEnumerable<DataLocalizedString>> GetDescriptorsAsync();

    protected async Task<IEnumerable<Models.AdminMenu>> GetAdminMenuListAsync()
    {
        var adminMenuList = await _adminMenuService.GetAdminMenuListAsync();

        return adminMenuList.AdminMenu;
    }
}
