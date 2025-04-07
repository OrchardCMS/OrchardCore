using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Localization;

public class RequestLocalizationOptionsConfigurations : IConfigureOptions<RequestLocalizationOptions>
{
    private readonly ShellSettings _shellSettings;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public RequestLocalizationOptionsConfigurations(ShellSettings shellSettings, IServiceScopeFactory serviceScopeFactory)
    {
        _shellSettings = shellSettings;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public void Configure(RequestLocalizationOptions options)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var adminOptions = scope.ServiceProvider.GetRequiredService<IOptions<AdminOptions>>().Value;

        options.AddInitialRequestCultureProvider(new AdminCookieCultureProvider(_shellSettings, adminOptions));
    }
}
