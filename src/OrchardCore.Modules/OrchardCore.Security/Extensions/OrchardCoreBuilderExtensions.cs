using Microsoft.Extensions.Configuration;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Security.Settings;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OrchardCoreBuilderExtensions
    {
        public static OrchardCoreBuilder ConfigureSecuritySettings(this OrchardCoreBuilder builder, bool overrideAdminSettings = true)
        {
            builder.ConfigureServices((tenantServices, serviceProvider) =>
            {
                var configurationSection = serviceProvider.GetRequiredService<IShellConfiguration>().GetSection("OrchardCore_Security");

                tenantServices.PostConfigure<SecuritySettings>(settings =>
                {
                    if (!overrideAdminSettings)
                    {
                        return;
                    }

                    configurationSection.Bind(settings);

                    settings.FromConfiguration = true;
                });
            });

            return builder;
        }
    }
}
