using Microsoft.Extensions.Configuration;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.GitHub.Settings;

namespace Microsoft.Extensions.DependencyInjection;

public static class OrchardCoreBuilderExtensions
{
    public static OrchardCoreBuilder ConfigureGitHubSettings(this OrchardCoreBuilder builder)
    {
        builder.ConfigureServices((tenantServices, serviceProvider) =>
        {
            var configuration = serviceProvider.GetRequiredService<IShellConfiguration>();

            tenantServices.PostConfigure<GitHubAuthenticationSettings>(settings =>
            {
                // The 'OrchardCore_GitHub' section is deprecated and will be removed in a future major version, use 'Authentication:GitHub' instead.
                configuration.GetSectionCompat("Authentication:GitHub", "OrchardCore_GitHub").Bind(settings);
            });
        });

        return builder;
    }
}
