using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities;
using OrchardCore.Settings;

namespace OrchardCore.Themes.Drivers;

public class ToggleThemeNavbarDisplayDriver : DisplayDriver<Navbar>
{
    private readonly ISiteService _siteService;

    public ToggleThemeNavbarDisplayDriver(ISiteService siteService)
    {
        _siteService = siteService;
    }

    public override IDisplayResult Display(Navbar model)
    {
        return View("ToggleTheme", model)
            .RenderWhen(async () => (await _siteService.GetSiteSettingsAsync()).As<AdminSettings>().DisplayThemeToggler)
            .Location("Detail", "Content:10")
            .Location("DetailAdmin", "Content:10");
    }
}
