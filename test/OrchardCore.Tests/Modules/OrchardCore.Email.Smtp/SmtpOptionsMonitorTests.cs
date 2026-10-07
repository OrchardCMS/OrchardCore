using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Email;
using OrchardCore.Email.Services;
using OrchardCore.Email.Smtp.Services;
using OrchardCore.Entities;
using OrchardCore.Environment.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Modules.OrchardCore.Email.Smtp;

public class SmtpOptionsMonitorTests
{
    [Fact]
    public async Task RequestUpdate_ShouldRefreshSmtpOptionsWithoutReleasingTenant()
    {
        using var context = new SiteContext()
            .WithRecipe("SaaS");

        await context.InitializeAsync();

        await context.UsingTenantScopeAsync(async scope =>
        {
            var shellFeaturesManager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            var availableFeatures = await shellFeaturesManager.GetAvailableFeaturesAsync();
            var featuresToEnable = availableFeatures.Where(feature => feature.Id == "OrchardCore.Email.Smtp");

            await shellFeaturesManager.EnableFeaturesAsync(featuresToEnable, force: true);
        });

        await context.WaitForDeferredTasksAsync(CancellationToken.None);

        long shellContextTicks = 0;

        await context.UsingTenantScopeAsync(scope =>
        {
            shellContextTicks = scope.ShellContext.UtcTicks;

            Assert.False(scope.ServiceProvider.GetRequiredService<IOptionsMonitor<SmtpOptions>>().CurrentValue.IsEnabled);

            return Task.CompletedTask;
        });

        await context.UsingTenantScopeAsync(async scope =>
        {
            var siteService = scope.ServiceProvider.GetRequiredService<ISiteService>();
            var notifier = scope.ServiceProvider.GetRequiredService<IOptionsUpdateNotifier>();

            var site = await siteService.LoadSiteSettingsAsync();

            site.Put(new SmtpSettings
            {
                IsEnabled = true,
                DefaultSender = "admin@example.com",
                DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory,
                PickupDirectoryLocation = "Email",
            });

            site.Put(new EmailSettings
            {
                DefaultProviderName = SmtpEmailProvider.TechnicalName,
            });

            notifier
                .RequestUpdate<SmtpOptions>()
                .RequestUpdate<EmailProviderOptions>()
                .RequestUpdate<EmailOptions>();

            await siteService.UpdateSiteSettingsAsync(site);
        });

        await context.WaitForDeferredTasksAsync(CancellationToken.None);

        await context.UsingTenantScopeAsync(async scope =>
        {
            Assert.Equal(shellContextTicks, scope.ShellContext.UtcTicks);

            var smtpOptions = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<SmtpOptions>>().CurrentValue;
            Assert.True(smtpOptions.IsEnabled);
            Assert.Equal("admin@example.com", smtpOptions.DefaultSender);
            Assert.Equal(SmtpDeliveryMethod.SpecifiedPickupDirectory, smtpOptions.DeliveryMethod);

            var emailOptions = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<EmailOptions>>().CurrentValue;
            Assert.Equal(SmtpEmailProvider.TechnicalName, emailOptions.DefaultProviderName);

            var providerOptions = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<EmailProviderOptions>>().CurrentValue;
            Assert.True(providerOptions.Providers[SmtpEmailProvider.TechnicalName].IsEnabled);

            var providerResolver = scope.ServiceProvider.GetRequiredService<IEmailProviderResolver>();
            var provider = await providerResolver.GetAsync();

            Assert.IsType<SmtpEmailProvider>(provider);
        });
    }
}
