using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace OrchardCore.Extensions;

/// <summary>
/// Applies the configured cookie names of the tenant to the ASP.NET Core cookies.
/// </summary>
internal sealed class CookieNamesConfiguration :
    IConfigureOptions<CookieTempDataProviderOptions>,
    IConfigureOptions<CookiePolicyOptions>,
    IPostConfigureOptions<RequestLocalizationOptions>
{
    private readonly CookieNameOptions _cookieNames;

    public CookieNamesConfiguration(IOptions<CookieNameOptions> cookieNames)
    {
        _cookieNames = cookieNames.Value;
    }

    public void Configure(CookieTempDataProviderOptions options)
        => options.Cookie.Name = _cookieNames.TempData;

    public void Configure(CookiePolicyOptions options)
        => options.ConsentCookie.Name = _cookieNames.Consent;

    public void PostConfigure(string name, RequestLocalizationOptions options)
    {
        foreach (var provider in options.RequestCultureProviders)
        {
            // Derived providers, e.g. the admin culture provider, use their own cookie.
            if (provider.GetType() == typeof(CookieRequestCultureProvider))
            {
                ((CookieRequestCultureProvider)provider).CookieName = _cookieNames.Culture;
            }
        }
    }
}
