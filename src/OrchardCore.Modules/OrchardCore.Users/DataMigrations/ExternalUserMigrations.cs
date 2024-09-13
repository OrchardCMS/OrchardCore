using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Data.Migration;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Settings;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.DataMigrations;

public sealed class ExternalUserMigrations : DataMigration
{
#pragma warning disable CA1822 // Mark members as static
    public int Create()
#pragma warning restore CA1822 // Mark members as static
#pragma warning disable CS0618 // Type or member is obsolete
    {
        ShellScope.AddDeferredTask(async scope =>
        {
            var siteService = scope.ServiceProvider.GetRequiredService<ISiteService>();

            var site = await siteService.LoadSiteSettingsAsync();

            var registrationSettings = site.As<RegistrationSettings>();

            registrationSettings.AllowSiteRegistration = registrationSettings.UsersCanRegister == UserRegistrationType.AllowRegistration;

            site.Put(registrationSettings);

            site.Put(new ExternalAuthenticationSettings
            {
                NoUsername = registrationSettings.NoUsernameForExternalUsers,
                NoEmail = registrationSettings.NoEmailForExternalUsers,
                NoPassword = registrationSettings.NoPasswordForExternalUsers,
                GenerateUsernameScript = registrationSettings.GenerateUsernameScript,
                UseScriptToGenerateUsername = registrationSettings.UseScriptToGenerateUsername
            });

            var loginSettings = site.As<LoginSettings>();

            site.Put(new ExternalUserLoginSettings
            {
                UseExternalProviderIfOnlyOneDefined = loginSettings.UseExternalProviderIfOnlyOneDefined,
            });

            await siteService.UpdateSiteSettingsAsync(site);

            if (registrationSettings.UsersCanRegister == UserRegistrationType.AllowOnlyExternalUsers)
            {
                var featuresManager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();

                if (await featuresManager.IsFeatureEnabledAsync(UserConstants.Features.ExternalAuthentication))
                {
                    return;
                }

                await featuresManager.EnableFeaturesAsync(UserConstants.Features.ExternalAuthentication);
            }
        });

        return 1;
    }
#pragma warning restore CS0618 // Type or member is obsolete
}
