using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Themes.Models;

namespace OrchardCore.Themes.TheAdmin.Drivers;

/// <summary>
/// Adds a "Schemes" button to the card of TheAdmin on Design > Themes.
/// </summary>
internal sealed class ThemeEntrySchemesDisplayDriver : DisplayDriver<ThemeEntry>
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ThemeEntrySchemesDisplayDriver(
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor)
    {
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
    }

    public override IDisplayResult Display(ThemeEntry model, BuildDisplayContext context)
        => View("ThemeEntry_SummaryAdmin__ButtonsSchemes", model)
            .RenderWhen(async () => model.Extension is not null
                && string.Equals(model.Extension.Id, TheAdminConstants.ThemeId, StringComparison.OrdinalIgnoreCase)
                && _httpContextAccessor.HttpContext?.User is { } user
                && await _authorizationService.AuthorizeAsync(user, Permissions.ApplyTheme))
            .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "FooterEnd:4");
}
