using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Data.Migration;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Settings;

namespace OrchardCore.Email.Migrations;

public sealed class EmailMigrations : DataMigration
{
    private const string SmtpFeatureId = "OrchardCore.Email.Smtp";

#pragma warning disable CA1822 // Member can be static
    public int Create()
#pragma warning restore CA1822
    {
        // In version 2.0, the OrchardCore.Email.Smtp was split from OrchardCore.Email. To ensure we keep the change
        // backward compatible, we added this migration step to auto-enable the new SMTP feature for sites that use the
        // Email service and have SmtpSettings.
        ShellScope.AddDeferredTask(async scope =>
        {
            var featuresManager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();

            if (await featuresManager.IsFeatureEnabledAsync(SmtpFeatureId))
            {
                return;
            }

            var siteService = scope.ServiceProvider.GetRequiredService<ISiteService>();
            var smtpSettings = await siteService.GetSettingsAsync<SmtpSettings>();

            if (!string.IsNullOrEmpty(smtpSettings.DefaultSender) ||
                scope.ServiceProvider.GetService<IOptions<SmtpOptions>>()?.Value.ConfigurationExists() == true)
            {
                // Enable the SMTP feature.
                await featuresManager.EnableFeaturesAsync(SmtpFeatureId);
            }
        });

        return 1;
    }
}
