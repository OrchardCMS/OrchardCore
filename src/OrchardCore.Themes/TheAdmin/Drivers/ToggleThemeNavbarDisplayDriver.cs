using OrchardCore.Admin;
using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;

namespace OrchardCore.Themes.TheAdmin.Drivers;

public sealed class ToggleThemeNavbarDisplayDriver : DisplayDriver<Navbar>
{
    private readonly ISiteService _siteService;
    private readonly IAdminThemeService _adminThemeService;

    public ToggleThemeNavbarDisplayDriver(
        ISiteService siteService,
        IAdminThemeService adminThemeService)
    {
        _siteService = siteService;
        _adminThemeService = adminThemeService;
    }

    public override IDisplayResult Display(Navbar model, BuildDisplayContext context)
    {
        return View("ToggleTheme", model)
            .RenderWhen(async () => (await _siteService.GetSettingsAsync<AdminSettings>()).DisplayThemeToggler && await _adminThemeService.GetAdminThemeNameAsync() == "TheAdmin")
            .Location("DetailAdmin", "Content:10");
    }
}
