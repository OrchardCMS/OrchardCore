using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;

namespace OrchardCore.Localization;

internal sealed class RequestLocalizationOptionsConfigurations : IConfigureOptions<RequestLocalizationOptions>
{
    private readonly CookieNameOptions _cookieNameOptions;
    private readonly AdminOptions _adminOptions;

    public RequestLocalizationOptionsConfigurations(
        IOptions<CookieNameOptions> cookieNameOptions,
        IOptions<AdminOptions> adminOptions)
    {
        _cookieNameOptions = cookieNameOptions.Value;
        _adminOptions = adminOptions.Value;
    }

    public void Configure(RequestLocalizationOptions options)
    {
        options.AddInitialRequestCultureProvider(new AdminCookieCultureProvider(_cookieNameOptions, _adminOptions));
    }
}
