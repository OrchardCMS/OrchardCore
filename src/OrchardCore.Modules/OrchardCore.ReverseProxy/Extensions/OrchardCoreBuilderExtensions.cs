using System;
using Microsoft.Extensions.Configuration;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.ReverseProxy.Settings;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OrchardCoreBuilderExtensions
    {
        [Obsolete]
        public static OrchardCoreBuilder ConfigureReverseProxySettings(this OrchardCoreBuilder builder)
        {
            builder.ConfigureServices((tenantServices, serviceProvider) =>
            {
                var configurationSection = serviceProvider.GetRequiredService<IShellConfiguration>().GetSection("OrchardCore_ReverseProxy");

                tenantServices.PostConfigure<ReverseProxySettings>(settings => configurationSection.Bind(settings));
            });

            return builder;
        }

        public static IServiceCollection ConfigureReverseProxySettings(this IServiceCollection services)
        {
            services.AddOptions<ReverseProxySettings>()
                .Configure<IShellConfiguration>(
                    (options, shellConfiguration) => shellConfiguration.GetSection("OrchardCore_ReverseProxy").Bind(options)
                );

            return services;
        }
    }
}
