using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Facebook;
using OrchardCore.Facebook.Settings;

namespace Microsoft.Extensions.DependencyInjection;

public static class OrchardCoreBuilderExtensions
{
    public static OrchardCoreBuilder ConfigureFacebookSettings(this OrchardCoreBuilder builder)
    {
        builder.ConfigureServices((tenantServices, serviceProvider) =>
        {
            // The 'OrchardCore_Facebook' section is deprecated and will be removed in a future major version, use 'Facebook' instead.
            var configurationSection = serviceProvider.GetRequiredService<IShellConfiguration>()
                .GetSectionCompat("Facebook", "OrchardCore_Facebook");

            tenantServices
                .AddOptions<FacebookSettings>()
                .PostConfigure<IDataProtectionProvider>((settings, dataProtectionProvider) =>
                {
                    configurationSection.Bind(settings);

                    // Secrets are consumed protected, as when they are saved from the admin settings, so protect the configured ones.
#pragma warning disable CS0618 // Protect legacy credentials supplied through configuration.
                    var appSecret = configurationSection[nameof(FacebookSettings.AppSecret)];

                    if (!string.IsNullOrWhiteSpace(appSecret))
                    {
                        settings.AppSecret = dataProtectionProvider.CreateProtector(FacebookConstants.Features.Core).Protect(appSecret);
                    }
#pragma warning restore CS0618
                });
        });

        return builder;
    }
}
