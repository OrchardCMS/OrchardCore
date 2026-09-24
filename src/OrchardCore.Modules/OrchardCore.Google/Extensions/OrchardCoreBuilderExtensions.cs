using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Google;
using OrchardCore.Google.Authentication.Settings;

namespace Microsoft.Extensions.DependencyInjection;

public static class OrchardCoreBuilderExtensions
{
    public static OrchardCoreBuilder ConfigureGoogleSettings(this OrchardCoreBuilder builder)
    {
        builder.ConfigureServices((tenantServices, serviceProvider) =>
        {
            // The 'OrchardCore_Google' section is deprecated and will be removed in a future major version, use 'Authentication:Google' instead.
            var configurationSection = serviceProvider.GetRequiredService<IShellConfiguration>()
                .GetSectionCompat("Authentication:Google", "OrchardCore_Google");

            tenantServices
                .AddOptions<GoogleAuthenticationSettings>()
                .PostConfigure<IDataProtectionProvider>((settings, dataProtectionProvider) =>
                {
                    configurationSection.Bind(settings);

                    // Secrets are consumed protected, as when they are saved from the admin settings, so protect the configured ones.
                    var clientSecret = configurationSection[nameof(GoogleAuthenticationSettings.ClientSecret)];

                    if (!string.IsNullOrWhiteSpace(clientSecret))
                    {
                        settings.ClientSecret = dataProtectionProvider.CreateProtector(GoogleConstants.Features.GoogleAuthentication).Protect(clientSecret);
                    }
                });
        });

        return builder;
    }
}
