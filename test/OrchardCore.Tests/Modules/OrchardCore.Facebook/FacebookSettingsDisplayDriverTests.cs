using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Facebook;
using OrchardCore.Facebook;
using OrchardCore.Facebook.Drivers;
using OrchardCore.Facebook.Login.Drivers;
using OrchardCore.Facebook.Login.Settings;
using OrchardCore.Facebook.Login.ViewModels;
using OrchardCore.Facebook.Settings;
using OrchardCore.Facebook.ViewModels;
using OrchardCore.ResourceManagement;
using OrchardCore.Settings;
using OrchardCore.Tests.OptionsMonitoring;
using OrchardCore.Tests.Stubs;

namespace OrchardCore.Tests.Modules.OrchardCore.Facebook;

public class FacebookSettingsDisplayDriverTests
{
    [Fact]
    public async Task UpdateAsync_ValidModel_RequestsFacebookOptionsUpdates()
    {
        var notifier = new RecordingOptionsUpdateNotifier();

        await UpdateAsync(notifier, isValid: true);

        Assert.Equal(
        [
            (typeof(FacebookSettings), ""),
            (typeof(ResourceManagementOptions), ""),
            (typeof(FacebookOptions), FacebookDefaults.AuthenticationScheme),
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

    [Fact]
    public async Task UpdateLoginAsync_Model_RequestsFacebookLoginOptionsUpdates()
    {
        var notifier = new RecordingOptionsUpdateNotifier();

        var driver = new FacebookLoginSettingsDisplayDriver(
            notifier,
            SettingsDriverTestHelper.AuthorizeAll(),
            SettingsDriverTestHelper.HttpContextAccessor());

        var context = SettingsDriverTestHelper.CreateUpdateContext(
            new FacebookLoginSettingsViewModel { CallbackPath = "/signin-facebook" },
            FacebookConstants.Features.Login);

        await driver.UpdateAsync(new SiteSettings(), new FacebookLoginSettings(), context);

        Assert.Equal(
        [
            (typeof(FacebookOptions), FacebookDefaults.AuthenticationScheme),
            (typeof(AuthenticationOptions), ""),
        ], notifier.Requests);
    }

    private static async Task UpdateAsync(RecordingOptionsUpdateNotifier notifier, bool isValid)
    {
        var driver = new FacebookSettingsDisplayDriver(
            notifier,
            SettingsDriverTestHelper.AuthorizeAll(),
            new EphemeralDataProtectionProvider(),
            SettingsDriverTestHelper.HttpContextAccessor(),
            NullLogger<FacebookSettingsDisplayDriver>.Instance);

        var context = SettingsDriverTestHelper.CreateUpdateContext(
            new FacebookSettingsViewModel { AppId = "app", AppSecret = "secret" },
            FacebookConstants.Features.Core,
            isValid);

        await driver.UpdateAsync(new SiteSettings(), new FacebookSettings(), context);
    }
}
