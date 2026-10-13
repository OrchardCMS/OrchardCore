using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Twitter;
using OrchardCore.Entities;
using OrchardCore.Environment.Cache;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Tests.OptionsMonitoring;
using OrchardCore.Twitter;
using OrchardCore.Twitter.Settings;
using OrchardCore.Twitter.Signin.Settings;

namespace OrchardCore.Tests.Modules.OrchardCore.Twitter;

public class TwitterOptionsTests
{
    [Fact]
    public async Task UpdateSettings_ValidSettings_AddsSchemeWithoutReleasingTenant()
    {
        using var context = await TenantOptionsTestHelper.CreateSiteWithFeaturesAsync(TwitterConstants.Features.Twitter, TwitterConstants.Features.Signin);

        var shellContextTicks = await TenantOptionsTestHelper.GetShellContextTicksAsync(context);

        Assert.Null(await TenantOptionsTestHelper.GetSchemeAsync(context, TwitterDefaults.AuthenticationScheme));

        await UpdateSettingsAsync(context, "key-1");

        Assert.NotNull(await TenantOptionsTestHelper.GetSchemeAsync(context, TwitterDefaults.AuthenticationScheme));
        Assert.Equal("key-1", await GetConsumerKeyAsync(context));
        Assert.Equal(shellContextTicks, await TenantOptionsTestHelper.GetShellContextTicksAsync(context));
    }

    [Fact]
    public async Task UpdateSettings_ConsumerKeyChanged_UpdatesHandlerAndClientWithoutReleasingTenant()
    {
        using var context = await TenantOptionsTestHelper.CreateSiteWithFeaturesAsync(TwitterConstants.Features.Twitter, TwitterConstants.Features.Signin);

        await UpdateSettingsAsync(context, "key-1");

        Assert.Equal("key-1", await GetConsumerKeyAsync(context));

        var shellContextTicks = await TenantOptionsTestHelper.GetShellContextTicksAsync(context);

        await UpdateSettingsAsync(context, "key-2");

        Assert.Equal("key-2", await GetConsumerKeyAsync(context));
        Assert.Equal("key-2", await TenantOptionsTestHelper.GetAsync(context, serviceProvider =>
            serviceProvider.GetRequiredService<IOptionsMonitor<TwitterSettings>>().CurrentValue.ConsumerKey));

        Assert.Equal(shellContextTicks, await TenantOptionsTestHelper.GetShellContextTicksAsync(context));
    }

    [Fact]
    public async Task UpdateSigninSettings_CallbackPathChanged_UpdatesHandlerWithoutReleasingTenant()
    {
        using var context = await TenantOptionsTestHelper.CreateSiteWithFeaturesAsync(TwitterConstants.Features.Twitter, TwitterConstants.Features.Signin);

        await UpdateSettingsAsync(context, "key-1");

        Assert.NotEqual("/signin-updated", await GetCallbackPathAsync(context));

        var shellContextTicks = await TenantOptionsTestHelper.GetShellContextTicksAsync(context);

        await TenantOptionsTestHelper.UpdateSiteAsync(
            context,
            (site, _) => site.Alter<TwitterSigninSettings>(settings => settings.CallbackPath = "/signin-updated"),
            notifier => notifier.RequestUpdate<TwitterOptions>(TwitterDefaults.AuthenticationScheme));

        Assert.Equal("/signin-updated", await GetCallbackPathAsync(context));
        Assert.Equal(shellContextTicks, await TenantOptionsTestHelper.GetShellContextTicksAsync(context));
    }

    private static Task<string> GetCallbackPathAsync(SiteContext context)
        => TenantOptionsTestHelper.GetAsync(context, serviceProvider =>
            serviceProvider.GetRequiredService<IOptionsMonitor<TwitterOptions>>().Get(TwitterDefaults.AuthenticationScheme).CallbackPath.Value);

    [Fact]
    public void ConfigureServices_SettingsFromConfiguration_ConfiguresSigninHandler()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddOptions();
        services.AddLocalization();
        services.AddHttpContextAccessor();
        services.AddSingleton<IDataProtectionProvider>(new EphemeralDataProtectionProvider());
        services.AddSingleton(Mock.Of<ISignal>(signal => signal.GetToken(It.IsAny<string>()) == NullChangeToken.Singleton));
        services.AddSingleton(new ShellSettings().AsDefaultShell().AsRunning());

        var siteService = new Mock<ISiteService>();
        siteService.Setup(service => service.GetSiteSettingsAsync()).ReturnsAsync(new SiteSettings());
        services.AddSingleton(siteService.Object);

        // Simulates the values bound from the 'X' configuration section.
        services
            .AddOptions<TwitterSettings>()
            .PostConfigure<IDataProtectionProvider>((settings, dataProtectionProvider) =>
            {
                var protector = dataProtectionProvider.CreateProtector(TwitterConstants.Features.Twitter);

                settings.ConsumerKey = "configured-key";
                settings.ConsumerSecret = protector.Protect("configured-secret");
                settings.AccessToken = "configured-token";
                settings.AccessTokenSecret = protector.Protect("configured-token-secret");
            });

        new TwitterStartup().ConfigureServices(services);
        new TwitterSigninStartup().ConfigureServices(services);

        using var serviceProvider = services.BuildServiceProvider();

        Assert.Contains(TwitterDefaults.AuthenticationScheme, serviceProvider.GetRequiredService<IOptions<AuthenticationOptions>>().Value.SchemeMap.Keys);

        var options = serviceProvider.GetRequiredService<IOptionsMonitor<TwitterOptions>>().Get(TwitterDefaults.AuthenticationScheme);

        Assert.Equal("configured-key", options.ConsumerKey);
        Assert.Equal("configured-secret", options.ConsumerSecret);
    }

    private static Task<string> GetConsumerKeyAsync(SiteContext context)
        => TenantOptionsTestHelper.GetAsync(context, serviceProvider =>
            serviceProvider.GetRequiredService<IOptionsMonitor<TwitterOptions>>().Get(TwitterDefaults.AuthenticationScheme).ConsumerKey);

    private static Task UpdateSettingsAsync(SiteContext context, string consumerKey)
        => TenantOptionsTestHelper.UpdateSiteAsync(
            context,
            (site, serviceProvider) =>
            {
                var protector = serviceProvider.GetRequiredService<IDataProtectionProvider>()
                    .CreateProtector(TwitterConstants.Features.Twitter);

                site.Alter<TwitterSettings>(settings =>
                {
                    settings.ConsumerKey = consumerKey;
                    settings.ConsumerSecret = protector.Protect("secret");
                    settings.AccessToken = "token";
                    settings.AccessTokenSecret = protector.Protect("token-secret");
                });
            },
            notifier => notifier
                .RequestUpdate<TwitterSettings>()
                .RequestUpdate<TwitterOptions>(TwitterDefaults.AuthenticationScheme)
                .RequestUpdate<AuthenticationOptions>());
}
