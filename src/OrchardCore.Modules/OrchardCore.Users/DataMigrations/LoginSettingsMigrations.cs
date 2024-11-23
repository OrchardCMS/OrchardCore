using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Data.Migration;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Settings;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.DataMigrations;

public sealed class LoginSettingsMigrations : DataMigration
{
    public int Create()
    {
        ShellScope.AddDeferredTask(async scope =>
        {
            var siteService = scope.ServiceProvider.GetRequiredService<ISiteService>();

            var site = await siteService.LoadSiteSettingsAsync();

            var registrationSettings = site.Properties[nameof(RegistrationSettings)]?.AsObject() ?? new JsonObject();

            var usersMustValidateEmail = registrationSettings["UsersMustValidateEmail"]?.GetValue<bool>() ?? false;

            var loginSettings = site.As<LoginSettings>();

            if (!loginSettings.UsersMustValidateEmail && usersMustValidateEmail)
            {
                loginSettings.UsersMustValidateEmail = true;

                site.Put(loginSettings);

                await siteService.UpdateSiteSettingsAsync(site);
            }
        });

        return 1;
    }
}
