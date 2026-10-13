using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using OrchardCore.Google;
using OrchardCore.Google.Authentication.Drivers;
using OrchardCore.Google.Authentication.Settings;
using OrchardCore.Google.Authentication.ViewModels;
using OrchardCore.Settings;
using OrchardCore.Tests.OptionsMonitoring;
using OrchardCore.Tests.Stubs;

namespace OrchardCore.Tests.Modules.OrchardCore.Google;

public class GoogleAuthenticationSettingsDisplayDriverTests
{
    [Fact]
    public async Task UpdateAsync_ValidModel_RequestsAuthenticationOptionsUpdates()
    {
        var notifier = new RecordingOptionsUpdateNotifier();

        await UpdateAsync(notifier, isValid: true);

        Assert.Equal(
        [
            (typeof(GoogleAuthenticationSettings), ""),
            (typeof(GoogleOptions), GoogleDefaults.AuthenticationScheme),
            (typeof(AuthenticationOptions), ""),
        ], notifier.Requests);
    }

    [Fact]
    public async Task UpdateAsync_InvalidModel_RequestsNoUpdate()
    {
        var notifier = new RecordingOptionsUpdateNotifier();

        await UpdateAsync(notifier, isValid: false);

        Assert.Empty(notifier.Requests);
    }

    private static async Task UpdateAsync(RecordingOptionsUpdateNotifier notifier, bool isValid)
    {
        var driver = new GoogleAuthenticationSettingsDisplayDriver(
            notifier,
            SettingsDriverTestHelper.AuthorizeAll(),
            new EphemeralDataProtectionProvider(),
            SettingsDriverTestHelper.HttpContextAccessor(),
            NullLogger<GoogleAuthenticationSettingsDisplayDriver>.Instance);

        var context = SettingsDriverTestHelper.CreateUpdateContext(
            new GoogleAuthenticationSettingsViewModel { ClientID = "client", ClientSecret = "secret" },
            GoogleConstants.Features.GoogleAuthentication,
            isValid);

        await driver.UpdateAsync(new SiteSettings(), new GoogleAuthenticationSettings(), context);
    }
}
