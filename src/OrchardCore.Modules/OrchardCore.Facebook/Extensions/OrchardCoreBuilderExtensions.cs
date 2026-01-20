using Microsoft.Extensions.Configuration;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Facebook.Settings;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OrchardCoreBuilderExtensions
    {
        public static OrchardCoreBuilder ConfigureFacebookSettings(this OrchardCoreBuilder builder)
        {
            builder.ConfigureServices((tenantServices, serviceProvider) =>
            {
                var configurationSection = serviceProvider.GetRequiredService<IShellConfiguration>().GetSection("OrchardCore_Facebook");

                tenantServices.PostConfigure<FacebookSettings>(settings => configurationSection.Bind(settings));
            });

            return builder;
        }
    }
}
