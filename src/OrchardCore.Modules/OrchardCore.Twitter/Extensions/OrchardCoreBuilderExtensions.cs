using System;
using Microsoft.Extensions.Configuration;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Twitter.Settings;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OrchardCoreBuilderExtensions
    {
        [Obsolete]
        public static OrchardCoreBuilder ConfigureTwitterSettings(this OrchardCoreBuilder builder)
        {
            builder.ConfigureServices((tenantServices, serviceProvider) =>
            {
                var configurationSection = serviceProvider.GetRequiredService<IShellConfiguration>().GetSection("OrchardCore_Twitter");

                tenantServices.PostConfigure<TwitterSettings>(settings => configurationSection.Bind(settings));
            });

            return builder;
        }

        public static IServiceCollection ConfigureTwitterSettings(this IServiceCollection services)
        {
            services.AddOptions<TwitterSettings>()
                .Configure<IShellConfiguration>(
                    (options, shellConfiguration) => shellConfiguration.GetSection("OrchardCore_Twitter").Bind(options)
                );

            return services;
        }
    }
}
