using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Email;
using OrchardCore.Email.Azure;
using OrchardCore.Email.Azure.Models;
using OrchardCore.Email.Azure.Services;
using OrchardCore.Email.Services;
using OrchardCore.Entities;
using OrchardCore.Environment.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Modules.OrchardCore.Email.Azure;

public class AzureEmailOptionsMonitorTests
{
    [Fact]
    public async Task RequestUpdate_ShouldRefreshAzureEmailOptionsWithoutReleasingTenant()
    {
        using var context = new SiteContext()
            .WithRecipe("SaaS");

        await context.InitializeAsync();

        await context.UsingTenantScopeAsync(async scope =>
        {
            var shellFeaturesManager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            var availableFeatures = await shellFeaturesManager.GetAvailableFeaturesAsync();
            var featuresToEnable = availableFeatures.Where(feature => feature.Id == "OrchardCore.Email.Azure");

            await shellFeaturesManager.EnableFeaturesAsync(featuresToEnable, force: true);
        });

        await context.WaitForDeferredTasksAsync(CancellationToken.None);

        long shellContextTicks = 0;

        await context.UsingTenantScopeAsync(scope =>
        {
            shellContextTicks = scope.ShellContext.UtcTicks;

            Assert.False(scope.ServiceProvider.GetRequiredService<IOptionsMonitor<AzureEmailOptions>>().CurrentValue.IsEnabled);
            Assert.Equal(AzureEmailProvider.TechnicalName, scope.ServiceProvider.GetRequiredService<IOptionsMonitor<EmailOptions>>().CurrentValue.DefaultProviderName);
            Assert.False(scope.ServiceProvider.GetRequiredService<IOptionsMonitor<EmailProviderOptions>>().CurrentValue.Providers[AzureEmailProvider.TechnicalName].IsEnabled);

            return Task.CompletedTask;
        });

        await context.UsingTenantScopeAsync(async scope =>
        {
            var siteService = scope.ServiceProvider.GetRequiredService<ISiteService>();
            var notifier = scope.ServiceProvider.GetRequiredService<IOptionsUpdateNotifier>();
            var dataProtectionProvider = scope.ServiceProvider.GetRequiredService<IDataProtectionProvider>();
            var protector = dataProtectionProvider.CreateProtector(AzureEmailOptionsConfiguration.ProtectorName);

            var site = await siteService.LoadSiteSettingsAsync();

            var azureEmailSettings = site.GetOrCreate<AzureEmailSettings>();
            azureEmailSettings.IsEnabled = true;
            azureEmailSettings.DefaultSender = "admin@example.com";
            azureEmailSettings.ConnectionString = protector.Protect("endpoint=https://example.communication.azure.com/;accesskey=test-key");
            site.Put(azureEmailSettings);

            var emailSettings = site.GetOrCreate<EmailSettings>();
            emailSettings.DefaultProviderName = AzureEmailProvider.TechnicalName;
            site.Put(emailSettings);

            notifier.RequestUpdate<AzureEmailOptions>();
            notifier.RequestUpdate<EmailProviderOptions>();
            notifier.RequestUpdate<EmailOptions>();

            await siteService.UpdateSiteSettingsAsync(site);
        });

        await context.WaitForDeferredTasksAsync(CancellationToken.None);

        await context.UsingTenantScopeAsync(async scope =>
        {
            Assert.Equal(shellContextTicks, scope.ShellContext.UtcTicks);

            var azureOptions = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<AzureEmailOptions>>().CurrentValue;
            Assert.True(azureOptions.IsEnabled);
            Assert.Equal("admin@example.com", azureOptions.DefaultSender);
            Assert.Equal("endpoint=https://example.communication.azure.com/;accesskey=test-key", azureOptions.ConnectionString);

            var providerOptions = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<EmailProviderOptions>>().CurrentValue;
            Assert.True(providerOptions.Providers[AzureEmailProvider.TechnicalName].IsEnabled);

            var emailOptions = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<EmailOptions>>().CurrentValue;
            Assert.Equal(AzureEmailProvider.TechnicalName, emailOptions.DefaultProviderName);

            var providerResolver = scope.ServiceProvider.GetRequiredService<IEmailProviderResolver>();
            var provider = await providerResolver.GetAsync();

            Assert.IsType<AzureEmailProvider>(provider);
        });
    }
}
