using Microsoft.Extensions.Configuration;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Security;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OrchardCoreBuilderExtensions
    {
        public static OrchardCoreBuilder ConfigureSecuritySettings(this OrchardCoreBuilder builder, bool overrideAdminSettings = true)
        {
            builder.ConfigureServices((tenantServices, serviceProvider) =>
            {
                var shellConfiguration = serviceProvider.GetRequiredService<IShellConfiguration>().GetSection("SecuritySettings");

                tenantServices.PostConfigure<SecuritySettings>(settings =>
                {
                    if (!overrideAdminSettings)
                    {
                        return;
                    }

                    shellConfiguration.Bind(settings);
                });
            });

            return builder;
        }
    }
}
