using AspNet.Security.OAuth.GitHub;
using Microsoft.AspNetCore.Authentication;
using OrchardCore.GitHub;
using OrchardCore.GitHub.Drivers;
using OrchardCore.GitHub.Settings;
using OrchardCore.GitHub.ViewModels;
using OrchardCore.Settings;
using OrchardCore.Tests.OptionsMonitoring;
using OrchardCore.Tests.Stubs;

namespace OrchardCore.Tests.Modules.OrchardCore.GitHub;

public class GitHubAuthenticationSettingsDisplayDriverTests
{
    [Fact]
    public async Task UpdateAsync_ValidModel_RequestsAuthenticationOptionsUpdates()
    {
        var notifier = new RecordingOptionsUpdateNotifier();

        await UpdateAsync(notifier, isValid: true);

        Assert.Equal(
        [
            (typeof(GitHubAuthenticationSettings), ""),
            (typeof(GitHubAuthenticationOptions), GitHubAuthenticationDefaults.AuthenticationScheme),
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
        var driver = new GitHubAuthenticationSettingsDisplayDriver(
            notifier,
            SettingsDriverTestHelper.AuthorizeAll(),
            new EphemeralDataProtectionProvider(),
            SettingsDriverTestHelper.HttpContextAccessor(),
            NullLogger<GitHubAuthenticationSettingsDisplayDriver>.Instance);

        var context = SettingsDriverTestHelper.CreateUpdateContext(
            new GitHubAuthenticationSettingsViewModel { ClientID = "client", ClientSecret = "secret" },
            GitHubConstants.Features.GitHubAuthentication,
            isValid);

        await driver.UpdateAsync(new SiteSettings(), new GitHubAuthenticationSettings(), context);
    }
}
