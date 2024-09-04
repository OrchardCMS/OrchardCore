using Microsoft.Extensions.Configuration;
using OrchardCore.Email;
using OrchardCore.Environment.Shell.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

public static class OrchardCoreBuilderExtensions
{
    [Obsolete("This extension is now obsolete and will be removed in the next release. You can safely stop using it, but please keep providing valid settings in the configuration provider for continued functionality.")]
    public static OrchardCoreBuilder ConfigureEmailSettings(this OrchardCoreBuilder builder)
    {
        builder.ConfigureServices((tenantServices, serviceProvider) =>
        {
            var configurationSection = serviceProvider.GetRequiredService<IShellConfiguration>().GetSection("OrchardCore_Email");

            tenantServices.Configure<DefaultSmtpOptions>(options =>
            {
                configurationSection.Bind(options);

                options.IsEnabled = options.ConfigurationExists();
            });
        });

        return builder;
    }
}
