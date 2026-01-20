using OrchardCore;
using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Themes.Services;

namespace TheTheme.Drivers;

public sealed class ToggleThemeNavbarDisplayDriver : DisplayDriver<Navbar>
{
    private readonly ISiteThemeService _siteThemeService;

    public ToggleThemeNavbarDisplayDriver(ISiteThemeService siteThemeService)
    {
        _siteThemeService = siteThemeService;
    }

    public override IDisplayResult Display(Navbar model, BuildDisplayContext context)
    {
        return View("ToggleTheme", model)
            .RenderWhen(async () => await _siteThemeService.GetSiteThemeNameAsync() == "TheTheme")
            .Location(OrchardCoreConstants.DisplayType.Detail, "Content:10");
    }
}
