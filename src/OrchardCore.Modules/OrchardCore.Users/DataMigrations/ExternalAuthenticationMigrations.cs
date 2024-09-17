using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Data.Migration;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Settings;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.DataMigrations;

public sealed class ExternalAuthenticationMigrations : DataMigration
{
    public int Create()
    {
        ShellScope.AddDeferredTask(async scope =>
        {
            var siteService = scope.ServiceProvider.GetRequiredService<ISiteService>();

            var site = await siteService.LoadSiteSettingsAsync();

            var registrationSettings = site.Properties[nameof(RegistrationSettings)]?.AsObject() ?? new JsonObject();

            site.Put(new ExternalRegistrationSettings
            {
                NoUsername = registrationSettings["NoUsernameForExternalUsers"]?.GetValue<bool>() ?? false,
                NoEmail = registrationSettings["NoEmailForExternalUsers"]?.GetValue<bool>() ?? false,
                NoPassword = registrationSettings["NoPasswordForExternalUsers"]?.GetValue<bool>() ?? false,
                GenerateUsernameScript = registrationSettings["GenerateUsernameScript"]?.ToString(),
                UseScriptToGenerateUsername = registrationSettings["UseScriptToGenerateUsername"]?.GetValue<bool>() ?? false,
            });

            var loginSettings = site.Properties[nameof(LoginSettings)]?.AsObject() ?? new JsonObject();

            site.Put(new ExternalLoginSettings
            {
                UseExternalProviderIfOnlyOneDefined = loginSettings["UseExternalProviderIfOnlyOneDefined"]?.GetValue<bool>() ?? false,
                UseScriptToSyncProperties = loginSettings["UseScriptToSyncRoles"]?.GetValue<bool>() ?? false,
                SyncPropertiesScript = loginSettings["SyncRolesScript"]?.ToString(),
            });

            await siteService.UpdateSiteSettingsAsync(site);
            var enumValue = registrationSettings["UsersCanRegister"]?.GetValue<int>();

            if (enumValue is not null && enumValue != 1)
            {
                var featuresManager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();

                if (await featuresManager.IsFeatureEnabledAsync(UserConstants.Features.UserRegistration))
                {
                    return;
                }

                await featuresManager.DisableFeaturesAsync(UserConstants.Features.UserRegistration);
            }
        });

        return 1;
    }
}
