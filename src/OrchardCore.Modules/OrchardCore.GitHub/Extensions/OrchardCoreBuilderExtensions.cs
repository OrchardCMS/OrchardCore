using System;
using Microsoft.Extensions.Configuration;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.GitHub.Settings;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OrchardCoreBuilderExtensions
    {
        [Obsolete]
        public static OrchardCoreBuilder ConfigureGitHubSettings(this OrchardCoreBuilder builder)
        {
            builder.ConfigureServices((tenantServices, serviceProvider) =>
            {
                var configurationSection = serviceProvider.GetRequiredService<IShellConfiguration>().GetSection("OrchardCore_GitHub");

                tenantServices.PostConfigure<GitHubAuthenticationSettings>(settings => configurationSection.Bind(settings));
            });

            return builder;
        }

        public static IServiceCollection ConfigureGitHubSettings(this IServiceCollection services)
        {
            services.AddOptions<GitHubAuthenticationSettings>()
                .Configure<IShellConfiguration>(
                    (options, shellConfiguration) => shellConfiguration.GetSection("OrchardCore_GitHub").Bind(options)
                );

            return services;
        }
    }
}
