using System.Threading.Tasks;
using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;

namespace OrchardCore.Themes.Drivers;

public class ToggleThemeNavbarDisplayDriver : DisplayDriver<Navbar>
{
    private readonly ISiteService _siteService;

    public ToggleThemeNavbarDisplayDriver(ISiteService siteService)
    {
        _siteService = siteService;
    }

    public override Task<IDisplayResult> DisplayAsync(Navbar model, BuildDisplayContext context)
    {
        return Task.FromResult<IDisplayResult>(
            View("ToggleTheme", model)
            .RenderWhen(async () => (await _siteService.GetSettingsAsync<AdminSettings>()).DisplayThemeToggler)
            .Location("Detail", "Content:10")
            .Location("DetailAdmin", "Content:10")
        );
    }
}
