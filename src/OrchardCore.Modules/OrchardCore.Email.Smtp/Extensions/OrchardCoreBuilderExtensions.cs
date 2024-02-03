using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrchardCore.Email.Smtp;
using OrchardCore.Email.Smtp.Services;
using OrchardCore.Environment.Shell.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

public static class OrchardCoreBuilderExtensions
{
    public static OrchardCoreBuilder ConfigureSmtpEmailSettings(this OrchardCoreBuilder builder)
    {
        builder.ConfigureServices((tenantServices, serviceProvider) =>
        {
            var configurationSection = serviceProvider.GetRequiredService<IShellConfiguration>().GetSection("OrchardCore_Email_Smtp");

            if (configurationSection.Value == null)
            {
                // Fall back to the old configuration section.
                configurationSection = serviceProvider.GetRequiredService<IShellConfiguration>().GetSection("OrchardCore_Email");

                var logger = serviceProvider.GetRequiredService<ILogger<SmtpEmailSettingsConfiguration>>();

                logger.LogWarning($"The {nameof(SmtpEmailSettings)} configuration section has been renamed to OrchardCore_Email_Smtp. Please update your configuration.");
            }

            tenantServices.PostConfigure<SmtpEmailSettings>(settings => configurationSection.Bind(settings));
        });

        return builder;
    }
}
