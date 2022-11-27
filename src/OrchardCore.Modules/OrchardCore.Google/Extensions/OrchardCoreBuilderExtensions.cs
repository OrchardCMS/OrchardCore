using Microsoft.Extensions.Configuration;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Google.Authentication.Settings;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OrchardCoreBuilderExtensions
    {
        public static OrchardCoreBuilder ConfigureGoogleSettings(this OrchardCoreBuilder builder)
        {
            builder.ConfigureServices((tenantServices, serviceProvider) =>
            {
                var configurationSection = serviceProvider.GetRequiredService<IShellConfiguration>().GetSection("OrchardCore_Google");

                tenantServices.PostConfigure<GoogleAuthenticationSettings>(settings => configurationSection.Bind(settings));
            });

            return builder;
        }
    }
}
