using Microsoft.Extensions.Configuration;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.GitHub.Settings;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OrchardCoreBuilderExtensions
    {
        public static OrchardCoreBuilder ConfigureGitHubSettings(this OrchardCoreBuilder builder)
        {
            builder.ConfigureServices((tenantServices, serviceProvider) =>
            {
                var configurationSection = serviceProvider.GetRequiredService<IShellConfiguration>().GetSection("OrchardCore_GitHub");

                tenantServices.PostConfigure<GitHubAuthenticationSettings>(settings => configurationSection.Bind(settings));
            });

            return builder;
        }
    }
}
