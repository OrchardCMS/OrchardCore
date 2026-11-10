using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Entities;
using OrchardCore.Environment.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.ReCaptcha.Configuration;
using OrchardCore.Settings;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Modules.OrchardCore.ReCaptcha;

public class ReCaptchaOptionsMonitorTests
{
    [Fact]
    public async Task RequestUpdate_ShouldRefreshReCaptchaOptionsWithoutReleasingTenant()
    {
        using var context = new SiteContext()
            .WithRecipe("SaaS");

        await context.InitializeAsync();

        await context.UsingTenantScopeAsync(async scope =>
        {
            var shellFeaturesManager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            var availableFeatures = await shellFeaturesManager.GetAvailableFeaturesAsync();
            var featuresToEnable = availableFeatures.Where(feature => feature.Id == "OrchardCore.ReCaptcha");

            await shellFeaturesManager.EnableFeaturesAsync(featuresToEnable, force: true);
        });

        await context.WaitForDeferredTasksAsync(CancellationToken.None);

        long shellContextTicks = 0;

        await context.UsingTenantScopeAsync(scope =>
        {
            shellContextTicks = scope.ShellContext.UtcTicks;

            var options = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<ReCaptchaSettings>>().CurrentValue;

            Assert.False(options.ConfigurationExists());

            return Task.CompletedTask;
        });

        await context.UsingTenantScopeAsync(async scope =>
        {
            var siteService = scope.ServiceProvider.GetRequiredService<ISiteService>();
            var notifier = scope.ServiceProvider.GetRequiredService<IOptionsUpdateNotifier>();

            var site = await siteService.LoadSiteSettingsAsync();
            site.Put(new ReCaptchaSettings
            {
                SiteKey = "site-key",
                SecretKey = "secret-key",
            });

            notifier.RequestUpdate<ReCaptchaSettings>();

            await siteService.UpdateSiteSettingsAsync(site);
        });

        await context.WaitForDeferredTasksAsync(CancellationToken.None);

        await context.UsingTenantScopeAsync(scope =>
        {
            Assert.Equal(shellContextTicks, scope.ShellContext.UtcTicks);

            var options = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<ReCaptchaSettings>>().CurrentValue;

            Assert.True(options.ConfigurationExists());
            Assert.Equal("site-key", options.SiteKey);
            Assert.Equal("secret-key", options.SecretKey);

            return Task.CompletedTask;
        });
    }
}
