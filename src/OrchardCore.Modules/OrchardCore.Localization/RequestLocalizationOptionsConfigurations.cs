using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Localization;

internal sealed class RequestLocalizationOptionsConfigurations : IConfigureOptions<RequestLocalizationOptions>
{
    private readonly ShellSettings _shellSettings;
    private readonly AdminOptions _adminOptions;

    public RequestLocalizationOptionsConfigurations(
        ShellSettings shellSettings,
        IOptions<AdminOptions> adminOptions)
    {
        _shellSettings = shellSettings;
        _adminOptions = adminOptions.Value;
    }

    public void Configure(RequestLocalizationOptions options)
    {
        options.AddInitialRequestCultureProvider(new AdminCookieCultureProvider(_shellSettings, _adminOptions));
    }
}
