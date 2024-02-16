using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Data.Migration;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Settings;

namespace OrchardCore.Email.Migrations;

public class EmailMigrations : DataMigration
{
    private const string SmtpFeatureId = "OrchardCore.Email.Smtp";

    public int Create()
    {
        // In version 1.9, the OrchardCore.Email.Smtp was split from OrchardCore.Email.
        // To ensure we keep the change backward compatible, we added this migration step
        // to auto enable the new SMTP feature for current site that are using the Email service
        // and the site has SmtpSettings.
        ShellScope.AddDeferredTask(async scope =>
        {
            var siteService = scope.ServiceProvider.GetRequiredService<ISiteService>();

            var featuresManager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();

            var enabledFeatures = await featuresManager.GetEnabledFeaturesAsync();

            if (enabledFeatures.Any(feature => feature.Id == SmtpFeatureId))
            {
                return;
            }

            var site = await siteService.GetSiteSettingsAsync();

            var smtpSettings = site.As<SmtpSettings>();

            var configuration = scope.ServiceProvider.GetRequiredService<IShellConfiguration>();

            var configSettings = configuration.GetSection("OrchardCore.Email").Get<SmtpSettings>();

            if (!string.IsNullOrEmpty(smtpSettings.DefaultSender) || !string.IsNullOrEmpty(configSettings?.DefaultSender))
            {
                // Enable the SMTP feature.
                var allFeatures = await featuresManager.GetAvailableFeaturesAsync();

                var smtpFeature = allFeatures.FirstOrDefault(feature => feature.Id == SmtpFeatureId);

                if (smtpFeature is null)
                {
                    return;
                }

                await featuresManager.EnableFeaturesAsync([smtpFeature]);
            }
        });
        return 1;
    }
}
