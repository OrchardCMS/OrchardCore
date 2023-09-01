using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities;
using OrchardCore.Settings;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Drivers;

public class ChangeEmailUserMenuDisplayDriver : DisplayDriver<UserMenu>
{
    private readonly ISiteService _siteService;

    public ChangeEmailUserMenuDisplayDriver(ISiteService siteService)
    {
        _siteService = siteService;
    }

    public override IDisplayResult Display(UserMenu model)
    {
        return View("UserMenuItems__ChangeEmail", model)
            .RenderWhen(async () => (await _siteService.GetSiteSettingsAsync()).As<ChangeEmailSettings>().AllowChangeEmail)
            .Location("Detail", "Content:20")
            .Location("DetailAdmin", "Content:20")
            .Differentiator("ChangeEmail");
    }
}
