using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Entities;
using OrchardCore.Environment.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;
using OrchardCore.Sms;
using OrchardCore.Sms.Models;
using OrchardCore.Sms.Services;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Modules.OrchardCore.Sms;

public class TwilioOptionsMonitorTests
{
    [Fact]
    public async Task RequestUpdate_ShouldRefreshTwilioOptionsWithoutReleasingTenant()
    {
        using var context = new SiteContext()
            .WithRecipe("SaaS");

        await context.InitializeAsync();

        await context.UsingTenantScopeAsync(async scope =>
        {
            var shellFeaturesManager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            var availableFeatures = await shellFeaturesManager.GetAvailableFeaturesAsync();
            var featuresToEnable = availableFeatures.Where(feature => feature.Id == "OrchardCore.Sms");

            await shellFeaturesManager.EnableFeaturesAsync(featuresToEnable, force: true);
        });

        await context.WaitForDeferredTasksAsync(CancellationToken.None);

        long shellContextTicks = 0;

        await context.UsingTenantScopeAsync(scope =>
        {
            shellContextTicks = scope.ShellContext.UtcTicks;

            Assert.False(scope.ServiceProvider.GetRequiredService<IOptionsMonitor<TwilioOptions>>().CurrentValue.IsEnabled);

            return Task.CompletedTask;
        });

        await context.UsingTenantScopeAsync(async scope =>
        {
            var siteService = scope.ServiceProvider.GetRequiredService<ISiteService>();
            var notifier = scope.ServiceProvider.GetRequiredService<IOptionsUpdateNotifier>();
            var dataProtectionProvider = scope.ServiceProvider.GetRequiredService<IDataProtectionProvider>();
            var protector = dataProtectionProvider.CreateProtector(TwilioSmsProvider.ProtectorName);

            var site = await siteService.LoadSiteSettingsAsync();
            site.Put(new SmsSettings
            {
                DefaultProviderName = TwilioSmsProvider.TechnicalName,
            });
            site.Put(new TwilioSettings
            {
                IsEnabled = true,
                PhoneNumber = "+15555555555",
                AccountSID = "account-sid",
                AuthToken = protector.Protect("auth-token"),
            });

            notifier
                .RequestUpdate<TwilioOptions>()
                .RequestUpdate<SmsProviderOptions>();

            await siteService.UpdateSiteSettingsAsync(site);
        });

        await context.WaitForDeferredTasksAsync(CancellationToken.None);

        await context.UsingTenantScopeAsync(async scope =>
        {
            Assert.Equal(shellContextTicks, scope.ShellContext.UtcTicks);

            var twilioOptions = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<TwilioOptions>>().CurrentValue;
            Assert.True(twilioOptions.IsEnabled);
            Assert.Equal("+15555555555", twilioOptions.PhoneNumber);
            Assert.Equal("account-sid", twilioOptions.AccountSID);
            Assert.Equal("auth-token", twilioOptions.AuthToken);

            var providerOptions = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<SmsProviderOptions>>().CurrentValue;
            Assert.True(providerOptions.Providers[TwilioSmsProvider.TechnicalName].IsEnabled);

            var providerResolver = scope.ServiceProvider.GetRequiredService<ISmsProviderResolver>();
            var provider = await providerResolver.GetAsync();

            Assert.IsType<TwilioSmsProvider>(provider);
        });
    }
}
