using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;

namespace OrchardCore.Localization;

internal sealed class RequestLocalizationOptionsConfigurations : IConfigureOptions<RequestLocalizationOptions>
{
    private readonly LocalizationCookieOptions _cookieOptions;
    private readonly AdminOptions _adminOptions;

    public RequestLocalizationOptionsConfigurations(
        IOptions<LocalizationCookieOptions> cookieOptions,
        IOptions<AdminOptions> adminOptions)
    {
        _cookieOptions = cookieOptions.Value;
        _adminOptions = adminOptions.Value;
    }

    public void Configure(RequestLocalizationOptions options)
    {
        options.AddInitialRequestCultureProvider(new AdminCookieCultureProvider(_cookieOptions, _adminOptions));
    }
}
