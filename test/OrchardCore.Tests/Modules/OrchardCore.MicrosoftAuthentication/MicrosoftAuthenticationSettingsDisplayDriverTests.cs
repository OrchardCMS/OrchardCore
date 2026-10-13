using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using OrchardCore.Microsoft.Authentication;
using OrchardCore.Microsoft.Authentication.Configuration;
using OrchardCore.Microsoft.Authentication.Drivers;
using OrchardCore.Microsoft.Authentication.Settings;
using OrchardCore.Microsoft.Authentication.ViewModels;
using OrchardCore.Settings;
using OrchardCore.Tests.OptionsMonitoring;
using OrchardCore.Tests.Stubs;

namespace OrchardCore.Tests.Modules.OrchardCore.MicrosoftAuthentication;

public class MicrosoftAuthenticationSettingsDisplayDriverTests
{
    [Fact]
    public async Task UpdateMicrosoftAccountAsync_ValidModel_RequestsMicrosoftAccountOptionsUpdates()
    {
        var notifier = new RecordingOptionsUpdateNotifier();

        await UpdateMicrosoftAccountAsync(notifier, isValid: true);

        Assert.Equal(
        [
            (typeof(MicrosoftAccountSettings), ""),
            (typeof(MicrosoftAccountOptions), MicrosoftAccountDefaults.AuthenticationScheme),
            (typeof(AuthenticationOptions), ""),
        ], notifier.Requests);
    }

    [Fact]
    public async Task UpdateMicrosoftAccountAsync_InvalidModel_RequestsNoUpdate()
    {
        var notifier = new RecordingOptionsUpdateNotifier();

        await UpdateMicrosoftAccountAsync(notifier, isValid: false);

        Assert.Empty(notifier.Requests);
    }

    [Fact]
    public async Task UpdateAzureADAsync_Model_RequestsAzureADOptionsUpdates()
    {
        var notifier = new RecordingOptionsUpdateNotifier();

        var driver = new AzureADSettingsDisplayDriver(
            notifier,
            SettingsDriverTestHelper.AuthorizeAll(),
            SettingsDriverTestHelper.HttpContextAccessor());

        var context = SettingsDriverTestHelper.CreateUpdateContext(
            new AzureADSettingsViewModel { DisplayName = "Entra ID", AppId = "app", TenantId = "tenant" },
            MicrosoftAuthenticationConstants.Features.AAD);

        await driver.UpdateAsync(new SiteSettings(), new AzureADSettings(), context);

        Assert.Equal(
        [
            (typeof(AzureADSettings), ""),
            (typeof(MicrosoftIdentityOptions), Constants.AzureAd),
            (typeof(OpenIdConnectOptions), AzureADOptionsConfiguration.AzureAdOpenIdConnectScheme),
            (typeof(AuthenticationOptions), ""),
        ], notifier.Requests);
    }

    private static async Task UpdateMicrosoftAccountAsync(RecordingOptionsUpdateNotifier notifier, bool isValid)
    {
        var driver = new MicrosoftAccountSettingsDisplayDriver(
            notifier,
            SettingsDriverTestHelper.AuthorizeAll(),
            new EphemeralDataProtectionProvider(),
            SettingsDriverTestHelper.HttpContextAccessor(),
            NullLogger<MicrosoftAccountSettingsDisplayDriver>.Instance);

        var context = SettingsDriverTestHelper.CreateUpdateContext(
            new MicrosoftAccountSettingsViewModel { AppId = "app", AppSecret = "secret" },
            MicrosoftAuthenticationConstants.Features.MicrosoftAccount,
            isValid);

        await driver.UpdateAsync(new SiteSettings(), new MicrosoftAccountSettings(), context);
    }
}
