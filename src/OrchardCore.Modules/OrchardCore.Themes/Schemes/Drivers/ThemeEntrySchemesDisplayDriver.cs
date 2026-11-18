using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Themes.Models;

namespace OrchardCore.Themes.Schemes.Drivers;

/// <summary>
/// Adds a "Schemes" button to the card of each theme that supports schemes on Design > Themes.
/// </summary>
internal sealed class ThemeEntrySchemesDisplayDriver : DisplayDriver<ThemeEntry>
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ThemeSchemesOptions _options;

    public ThemeEntrySchemesDisplayDriver(
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor,
        IOptions<ThemeSchemesOptions> options)
    {
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
        _options = options.Value;
    }

    public override IDisplayResult Display(ThemeEntry model, BuildDisplayContext context)
        => View("ThemeEntry_SummaryAdmin__ButtonsSchemes", model)
            .RenderWhen(async () => model.Extension is not null
                && _options.Themes.Contains(model.Extension.Id)
                && _httpContextAccessor.HttpContext?.User is { } user
                && await _authorizationService.AuthorizeAsync(user, Permissions.ApplyTheme))
            .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "FooterEnd:4");
}
