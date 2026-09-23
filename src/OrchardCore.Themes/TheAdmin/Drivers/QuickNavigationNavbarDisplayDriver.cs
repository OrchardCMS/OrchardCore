using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;

namespace OrchardCore.Themes.TheAdmin.Drivers;

public sealed class QuickNavigationNavbarDisplayDriver : DisplayDriver<Navbar>
{
    private readonly ISiteService _siteService;

    public QuickNavigationNavbarDisplayDriver(ISiteService siteService)
    {
        _siteService = siteService;
    }

    public override IDisplayResult Display(Navbar model, BuildDisplayContext context)
    {
        return View("QuickNavigationNavbarItem", model)
            .RenderWhen(static async (siteService) => (await siteService.GetSettingsAsync<AdminSettings>()).DisplayQuickNavigation, _siteService)
            .Location(OrchardCoreConstants.DisplayType.DetailAdmin, "Content:5");
    }
}
