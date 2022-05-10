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
                var shellConfiguration = serviceProvider.GetRequiredService<IShellConfiguration>().GetSection("OrchardCore_Security");

                tenantServices.PostConfigure<SecuritySettings>(settings =>
                {
                    if (!overrideAdminSettings)
                    {
                        return;
                    }

                    // Reset the settings to avoid merging with the current settings values
                    settings = new SecuritySettings
                    {
                        ContentSecurityPolicy = SecurityHeaderDefaults.ContentSecurityPolicy,
                        ContentTypeOptions = SecurityHeaderDefaults.ContentTypeOptions,
                        FrameOptions = SecurityHeaderDefaults.FrameOptions,
                        PermissionsPolicy = SecurityHeaderDefaults.PermissionsPolicy,
                        ReferrerPolicy = SecurityHeaderDefaults.ReferrerPolicy
                    };
                    shellConfiguration.Bind(settings);
                });
            });

            return builder;
        }
    }
}
