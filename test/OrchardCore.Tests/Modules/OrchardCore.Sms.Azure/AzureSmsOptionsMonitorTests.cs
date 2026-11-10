using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Entities;
using OrchardCore.Environment.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;
using OrchardCore.Sms;
using OrchardCore.Sms.Azure.Models;
using OrchardCore.Sms.Azure.Services;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Modules.OrchardCore.Sms.Azure;

public class AzureSmsOptionsMonitorTests
{
    [Fact]
    public async Task RequestUpdate_ShouldRefreshAzureSmsOptionsWithoutReleasingTenant()
    {
        using var context = new SiteContext()
            .WithRecipe("SaaS");

        await context.InitializeAsync();

        await context.UsingTenantScopeAsync(async scope =>
        {
            var shellFeaturesManager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            var availableFeatures = await shellFeaturesManager.GetAvailableFeaturesAsync();
            var featuresToEnable = availableFeatures.Where(feature => feature.Id == "OrchardCore.Sms.Azure");

            await shellFeaturesManager.EnableFeaturesAsync(featuresToEnable, force: true);
        });

        await context.WaitForDeferredTasksAsync(CancellationToken.None);

        long shellContextTicks = 0;

        await context.UsingTenantScopeAsync(scope =>
        {
            shellContextTicks = scope.ShellContext.UtcTicks;

            Assert.False(scope.ServiceProvider.GetRequiredService<IOptionsMonitor<AzureSmsOptions>>().CurrentValue.IsEnabled);

            return Task.CompletedTask;
        });

        await context.UsingTenantScopeAsync(async scope =>
        {
            var siteService = scope.ServiceProvider.GetRequiredService<ISiteService>();
            var notifier = scope.ServiceProvider.GetRequiredService<IOptionsUpdateNotifier>();
            var dataProtectionProvider = scope.ServiceProvider.GetRequiredService<IDataProtectionProvider>();
            var protector = dataProtectionProvider.CreateProtector(AzureSmsOptionsConfiguration.ProtectorName);

            var site = await siteService.LoadSiteSettingsAsync();
            site.Put(new SmsSettings
            {
                DefaultProviderName = AzureSmsProvider.TechnicalName,
            });
            site.Put(new AzureSmsSettings
            {
                IsEnabled = true,
                PhoneNumber = "+15555555555",
                ConnectionString = protector.Protect("endpoint=https://example.communication.azure.com/;accesskey=test-key"),
            });

            notifier
                .RequestUpdate<AzureSmsOptions>()
                .RequestUpdate<SmsProviderOptions>();

            await siteService.UpdateSiteSettingsAsync(site);
        });

        await context.WaitForDeferredTasksAsync(CancellationToken.None);

        await context.UsingTenantScopeAsync(async scope =>
        {
            Assert.Equal(shellContextTicks, scope.ShellContext.UtcTicks);

            var azureOptions = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<AzureSmsOptions>>().CurrentValue;
            Assert.True(azureOptions.IsEnabled);
            Assert.Equal("+15555555555", azureOptions.PhoneNumber);
            Assert.Equal("endpoint=https://example.communication.azure.com/;accesskey=test-key", azureOptions.ConnectionString);

            var providerOptions = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<SmsProviderOptions>>().CurrentValue;
            Assert.True(providerOptions.Providers[AzureSmsProvider.TechnicalName].IsEnabled);

            var providerResolver = scope.ServiceProvider.GetRequiredService<ISmsProviderResolver>();
            var provider = await providerResolver.GetAsync();

            Assert.IsType<AzureSmsProvider>(provider);
        });
    }
}
