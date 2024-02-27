using System;
using Microsoft.Extensions.Configuration;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Microsoft.Authentication.Settings;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OrchardCoreBuilderExtensions
    {
        [Obsolete]
        public static OrchardCoreBuilder ConfigureMicrosoftAccountSettings(this OrchardCoreBuilder builder)
        {
            builder.ConfigureServices((tenantServices, serviceProvider) =>
            {
                var configurationSection = serviceProvider.GetRequiredService<IShellConfiguration>().GetSection("OrchardCore_Microsoft_Authentication_MicrosoftAccount");

                tenantServices.PostConfigure<MicrosoftAccountSettings>(settings => configurationSection.Bind(settings));
            });

            return builder;
        }

        public static IServiceCollection ConfigureMicrosoftAccountSettings(this IServiceCollection services)
        {
            services.AddOptions<MicrosoftAccountSettings>()
                .Configure<IShellConfiguration>(
                    (options, shellConfiguration) => shellConfiguration.GetSection("OrchardCore_Microsoft_Authentication_MicrosoftAccount").Bind(options)
                );

            return services;
        }

        [Obsolete]
        public static OrchardCoreBuilder ConfigureAzureADSettings(this OrchardCoreBuilder builder)
        {
            builder.ConfigureServices((tenantServices, serviceProvider) =>
            {
                var configurationSection = serviceProvider.GetRequiredService<IShellConfiguration>().GetSection("OrchardCore_Microsoft_Authentication_AzureAD");

                tenantServices.PostConfigure<AzureADSettings>(settings => configurationSection.Bind(settings));
            });

            return builder;
        }

        public static IServiceCollection ConfigureAzureADSettings(this IServiceCollection services)
        {
            services.AddOptions<AzureADSettings>()
                .Configure<IShellConfiguration>(
                    (options, shellConfiguration) => shellConfiguration.GetSection("OrchardCore_Microsoft_Authentication_AzureAD").Bind(options)
                );

            return services;
        }
    }
}
