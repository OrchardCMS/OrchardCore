using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Theming;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Themes.Models;

namespace OrchardCore.Themes.TheAdmin.Drivers;

/// <summary>
/// Adds a "Schemes" button to the card of TheAdmin on Design > Themes, while TheAdmin is the theme of the page: the
/// schemes only style TheAdmin, and the shapes of a theme, the button's included, are only there while it is in use.
/// </summary>
internal sealed class ThemeEntrySchemesDisplayDriver : DisplayDriver<ThemeEntry>
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IThemeManager _themeManager;

    public ThemeEntrySchemesDisplayDriver(
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor,
        IThemeManager themeManager)
    {
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
        _themeManager = themeManager;
    }

    public override IDisplayResult Display(ThemeEntry model, BuildDisplayContext context)
        => View("ThemeEntrySchemesButton", model)
            .RenderWhen(async () => model.Extension is not null
                && string.Equals(model.Extension.Id, TheAdminConstants.ThemeId, StringComparison.OrdinalIgnoreCase)
                && string.Equals((await _themeManager.GetThemeAsync())?.Id, TheAdminConstants.ThemeId, StringComparison.OrdinalIgnoreCase)
                && _httpContextAccessor.HttpContext?.User is { } user
                && await _authorizationService.AuthorizeAsync(user, Permissions.ApplyTheme))
            .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "FooterEnd:4");
}
