using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Theming;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Extensions;
using OrchardCore.Settings;

namespace OrchardCore.Themes.TheOldAdmin.Drivers;

public sealed class ToggleThemeNavbarDisplayDriver : DisplayDriver<Navbar>
{
    /// <summary>
    /// The id of the theme's extension.
    /// </summary>
    private const string ThemeId = "TheOldAdmin";

    private readonly ISiteService _siteService;
    private readonly IThemeManager _themeManager;
    private readonly IExtensionManager _extensionManager;

    public ToggleThemeNavbarDisplayDriver(
        ISiteService siteService,
        IThemeManager themeManager,
        IExtensionManager extensionManager)
    {
        _siteService = siteService;
        _themeManager = themeManager;
        _extensionManager = extensionManager;
    }

    public override IDisplayResult Display(Navbar model, BuildDisplayContext context)
    {
        // A theme's drivers run while it is enabled, not only while it is the theme of the page: TheAdmin, which adds
        // its own toggle, is usually enabled too. So the toggle renders only while TheOldAdmin, or an admin theme built
        // on it, is the theme of the page.
        return View("ToggleTheme", model)
            .RenderWhen(async () => await IsThemeOfThePageAsync() && (await _siteService.GetSettingsAsync<AdminSettings>()).DisplayThemeToggler)
            .Location(OrchardCoreConstants.DisplayType.DetailAdmin, "Content:10");
    }

    private async Task<bool> IsThemeOfThePageAsync()
    {
        var theme = await _themeManager.GetThemeAsync();

        return theme is not null
            && (theme.Id == ThemeId
                || _extensionManager.GetFeatureDependencies(theme.Id).Any(feature => feature.Id == ThemeId));
    }
}
