using System;
using Microsoft.Extensions.Configuration;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Facebook.Settings;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OrchardCoreBuilderExtensions
    {
        [Obsolete]
        public static OrchardCoreBuilder ConfigureFacebookSettings(this OrchardCoreBuilder builder)
        {
            builder.ConfigureServices((tenantServices, serviceProvider) =>
            {
                var configurationSection = serviceProvider.GetRequiredService<IShellConfiguration>().GetSection("OrchardCore_Facebook");

                tenantServices.PostConfigure<FacebookSettings>(settings => configurationSection.Bind(settings));
            });

            return builder;
        }

        public static IServiceCollection ConfigureFacebookSettings(this IServiceCollection services)
        {
            services.AddOptions<FacebookSettings>()
                .Configure<IShellConfiguration>(
                    (options, shellConfiguration) => shellConfiguration.GetSection("OrchardCore_Facebook").Bind(options)
                );

            return services;
        }
    }
}
