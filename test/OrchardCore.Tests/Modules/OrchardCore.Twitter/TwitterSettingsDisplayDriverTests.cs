using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Twitter;
using OrchardCore.Settings;
using OrchardCore.Tests.OptionsMonitoring;
using OrchardCore.Tests.Stubs;
using OrchardCore.Twitter;
using OrchardCore.Twitter.Drivers;
using OrchardCore.Twitter.Settings;
using OrchardCore.Twitter.Signin.Drivers;
using OrchardCore.Twitter.Signin.Settings;
using OrchardCore.Twitter.Signin.ViewModels;
using OrchardCore.Twitter.ViewModels;

namespace OrchardCore.Tests.Modules.OrchardCore.Twitter;

public class TwitterSettingsDisplayDriverTests
{
    [Fact]
    public async Task UpdateAsync_ValidModel_RequestsTwitterOptionsUpdates()
    {
        var notifier = new RecordingOptionsUpdateNotifier();

        await UpdateAsync(notifier, isValid: true);

        Assert.Equal(
        [
            (typeof(TwitterSettings), ""),
            (typeof(TwitterOptions), TwitterDefaults.AuthenticationScheme),
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
    public async Task UpdateSigninAsync_Model_RequestsTwitterOptionsUpdate()
    {
        var notifier = new RecordingOptionsUpdateNotifier();

        var driver = new TwitterSigninSettingsDisplayDriver(
            notifier,
            SettingsDriverTestHelper.AuthorizeAll(),
            SettingsDriverTestHelper.HttpContextAccessor());

        var context = SettingsDriverTestHelper.CreateUpdateContext(
            new TwitterSigninSettingsViewModel { CallbackPath = "/signin-twitter" },
            TwitterConstants.Features.Signin);

        await driver.UpdateAsync(new SiteSettings(), new TwitterSigninSettings(), context);

        Assert.Equal([(typeof(TwitterOptions), TwitterDefaults.AuthenticationScheme)], notifier.Requests);
    }

    private static async Task UpdateAsync(RecordingOptionsUpdateNotifier notifier, bool isValid)
    {
        var driver = new TwitterSettingsDisplayDriver(
            notifier,
            SettingsDriverTestHelper.AuthorizeAll(),
            new EphemeralDataProtectionProvider(),
            SettingsDriverTestHelper.HttpContextAccessor(),
            NullLogger<TwitterSettingsDisplayDriver>.Instance);

        var context = SettingsDriverTestHelper.CreateUpdateContext(
            new TwitterSettingsViewModel { APIKey = "key", APISecretKey = "secret", AccessToken = "token", AccessTokenSecret = "token-secret" },
            TwitterConstants.Features.Twitter,
            isValid);

        await driver.UpdateAsync(new SiteSettings(), new TwitterSettings(), context);
    }
}
