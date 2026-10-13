using AspNet.Security.OAuth.GitHub;
using Microsoft.AspNetCore.Authentication;
using OrchardCore.Entities;
using OrchardCore.Environment.Cache;
using OrchardCore.GitHub;
using OrchardCore.GitHub.Settings;
using OrchardCore.Settings;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Tests.OptionsMonitoring;

namespace OrchardCore.Tests.Modules.OrchardCore.GitHub;

public class GitHubAuthenticationOptionsTests
{
    [Fact]
    public async Task UpdateSettings_ValidSettings_AddsSchemeWithoutReleasingTenant()
    {
        using var context = await TenantOptionsTestHelper.CreateSiteWithFeaturesAsync(GitHubConstants.Features.GitHubAuthentication);

        var shellContextTicks = await TenantOptionsTestHelper.GetShellContextTicksAsync(context);

        Assert.Null(await TenantOptionsTestHelper.GetSchemeAsync(context, GitHubAuthenticationDefaults.AuthenticationScheme));

        await UpdateSettingsAsync(context, "client-1");

        Assert.NotNull(await TenantOptionsTestHelper.GetSchemeAsync(context, GitHubAuthenticationDefaults.AuthenticationScheme));
        Assert.Contains("client_id=client-1", await TenantOptionsTestHelper.ChallengeAsync(context, GitHubAuthenticationDefaults.AuthenticationScheme));
        Assert.Equal(shellContextTicks, await TenantOptionsTestHelper.GetShellContextTicksAsync(context));
    }

    [Fact]
    public async Task UpdateSettings_ClientIdChanged_ChallengesWithNewClientIdWithoutReleasingTenant()
    {
        using var context = await TenantOptionsTestHelper.CreateSiteWithFeaturesAsync(GitHubConstants.Features.GitHubAuthentication);

        await UpdateSettingsAsync(context, "client-1");

        Assert.Contains("client_id=client-1", await TenantOptionsTestHelper.ChallengeAsync(context, GitHubAuthenticationDefaults.AuthenticationScheme));

        var shellContextTicks = await TenantOptionsTestHelper.GetShellContextTicksAsync(context);

        await UpdateSettingsAsync(context, "client-2");

        Assert.Contains("client_id=client-2", await TenantOptionsTestHelper.ChallengeAsync(context, GitHubAuthenticationDefaults.AuthenticationScheme));
        Assert.Equal(shellContextTicks, await TenantOptionsTestHelper.GetShellContextTicksAsync(context));
    }

    [Fact]
    public void ConfigureServices_SettingsFromConfiguration_ConfiguresHandler()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddOptions();
        services.AddSingleton<IDataProtectionProvider>(new EphemeralDataProtectionProvider());
        services.AddSingleton(Mock.Of<ISignal>(signal => signal.GetToken(It.IsAny<string>()) == NullChangeToken.Singleton));

        var siteService = new Mock<ISiteService>();
        siteService.Setup(service => service.GetSiteSettingsAsync()).ReturnsAsync(new SiteSettings());
        services.AddSingleton(siteService.Object);

        // Simulates the values bound from the 'Authentication:GitHub' configuration section.
        services
            .AddOptions<GitHubAuthenticationSettings>()
            .PostConfigure<IDataProtectionProvider>((settings, dataProtectionProvider) =>
            {
                settings.ClientID = "configured-client";
                settings.ClientSecret = dataProtectionProvider.CreateProtector(GitHubConstants.Features.GitHubAuthentication).Protect("configured-secret");
            });

        new GitHubLoginStartup().ConfigureServices(services);

        using var serviceProvider = services.BuildServiceProvider();

        Assert.Contains(GitHubAuthenticationDefaults.AuthenticationScheme, serviceProvider.GetRequiredService<IOptions<AuthenticationOptions>>().Value.SchemeMap.Keys);

        var options = serviceProvider.GetRequiredService<IOptionsMonitor<GitHubAuthenticationOptions>>().Get(GitHubAuthenticationDefaults.AuthenticationScheme);

        Assert.Equal("configured-client", options.ClientId);
        Assert.Equal("configured-secret", options.ClientSecret);
    }

    private static Task UpdateSettingsAsync(SiteContext context, string clientId)
        => TenantOptionsTestHelper.UpdateSiteAsync(
            context,
            (site, serviceProvider) =>
            {
                var protector = serviceProvider.GetRequiredService<IDataProtectionProvider>()
                    .CreateProtector(GitHubConstants.Features.GitHubAuthentication);

                site.Alter<GitHubAuthenticationSettings>(settings =>
                {
                    settings.ClientID = clientId;
                    settings.ClientSecret = protector.Protect("secret");
                });
            },
            notifier => notifier
                .RequestUpdate<GitHubAuthenticationSettings>()
                .RequestUpdate<GitHubAuthenticationOptions>(GitHubAuthenticationDefaults.AuthenticationScheme)
                .RequestUpdate<AuthenticationOptions>());
}
