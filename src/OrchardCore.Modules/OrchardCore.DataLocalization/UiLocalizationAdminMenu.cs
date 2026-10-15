using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.Localization;
using OrchardCore.Localization.Data;
using OrchardCore.Navigation;

namespace OrchardCore.DataLocalization;

/// <summary>Provides UI-translation navigation for authorized site translators.</summary>
public sealed class UiLocalizationAdminMenu : AdminNavigationProvider
{
    private readonly IStringLocalizer S;
    private readonly IAuthorizationService _authorization;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILocalizationService _localization;

    /// <summary>Creates the UI-translations navigation provider.</summary>
    /// <param name="localizer">The menu localizer.</param>
    /// <param name="authorization">The permission service.</param>
    /// <param name="httpContextAccessor">The current admin request.</param>
    /// <param name="localization">The configured cultures.</param>
    public UiLocalizationAdminMenu(
        IStringLocalizer<UiLocalizationAdminMenu> localizer,
        IAuthorizationService authorization,
        IHttpContextAccessor httpContextAccessor,
        ILocalizationService localization)
    {
        S = localizer;
        _authorization = authorization;
        _httpContextAccessor = httpContextAccessor;
        _localization = localization;
    }

    protected override async ValueTask BuildAsync(NavigationBuilder builder)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null)
        {
            return;
        }

        var authorized = await _authorization.AuthorizeAsync(user, DataLocalizationPermissions.ViewDynamicTranslations);
        if (!authorized)
        {
            foreach (var culture in await _localization.GetSupportedCulturesAsync())
            {
                if (await _authorization.AuthorizeAsync(user,
                    DataLocalizationPermissions.CreateCulturePermission(culture, CultureInfo.GetCultureInfo(culture).DisplayName)))
                {
                    authorized = true;
                    break;
                }
            }
        }

        if (!authorized)
        {
            return;
        }

        if (NavigationHelper.UseLegacyFormat())
        {
            builder.Add(S["Configuration"], configuration => configuration
                .Add(S["Settings"], settings => settings
                    .Add(S["Localization"], localization => localization
                        .Add(S["UI Translations"], S["UI Translations"].PrefixPosition(), translations => translations
                            .Action("Index", "UiTranslations", new { area = "OrchardCore.DataLocalization" })
                            .LocalNav()))));
            return;
        }

        builder.Add(S["Settings"], settings => settings
            .Add(S["Localization"], S["Localization"].PrefixPosition(), localization => localization
                .Add(S["UI Translations"], S["UI Translations"].PrefixPosition(), translations => translations
                    .Action("Index", "UiTranslations", new { area = "OrchardCore.DataLocalization" })
                    .LocalNav())));
    }
}
