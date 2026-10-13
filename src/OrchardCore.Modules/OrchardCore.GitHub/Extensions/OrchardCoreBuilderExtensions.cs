using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.GitHub;
using OrchardCore.GitHub.Settings;

namespace Microsoft.Extensions.DependencyInjection;

public static class OrchardCoreBuilderExtensions
{
    public static OrchardCoreBuilder ConfigureGitHubSettings(this OrchardCoreBuilder builder)
    {
        builder.ConfigureServices((tenantServices, serviceProvider) =>
        {
            // The 'OrchardCore_GitHub' section is deprecated and will be removed in a future major version, use 'Authentication:GitHub' instead.
            var configurationSection = serviceProvider.GetRequiredService<IShellConfiguration>()
                .GetSectionCompat("Authentication:GitHub", "OrchardCore_GitHub");

            tenantServices
                .AddOptions<GitHubAuthenticationSettings>()
                .PostConfigure<IDataProtectionProvider>((settings, dataProtectionProvider) =>
                {
                    configurationSection.Bind(settings);

                    // Secrets are consumed protected, as when they are saved from the admin settings, so protect the configured ones.
                    var clientSecret = configurationSection[nameof(GitHubAuthenticationSettings.ClientSecret)];

                    if (!string.IsNullOrWhiteSpace(clientSecret))
                    {
                        settings.ClientSecret = dataProtectionProvider.CreateProtector(GitHubConstants.Features.GitHubAuthentication).Protect(clientSecret);
                    }
                });
        });

        return builder;
    }
}
