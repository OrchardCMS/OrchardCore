using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Entities;
using OrchardCore.Environment.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Users.Models;

namespace OrchardCore.Tests.Modules.OrchardCore.Users;

public class ExternalLoginOptionsMonitorTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RequestUpdate_ShouldRefreshExternalLoginOptionsWithoutReleasingTenant(bool useManagement)
    {
        using var context = new SiteContext()
            .WithRecipe("SaaS");

        await context.InitializeAsync();

        await context.UsingTenantScopeAsync(async scope =>
        {
            var shellFeaturesManager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            var availableFeatures = await shellFeaturesManager.GetAvailableFeaturesAsync();
            var featuresToEnable = availableFeatures.Where(feature => feature.Id == "OrchardCore.GitHub.Authentication");

            await shellFeaturesManager.EnableFeaturesAsync(featuresToEnable, force: true);
        });

        await context.WaitForDeferredTasksAsync(CancellationToken.None);

        long shellContextTicks = 0;

        await context.UsingTenantScopeAsync(scope =>
        {
            shellContextTicks = scope.ShellContext.UtcTicks;

            Assert.False(scope.ServiceProvider.GetRequiredService<IOptionsMonitor<ExternalLoginOptions>>().CurrentValue.UseExternalProviderIfOnlyOneDefined);

            return Task.CompletedTask;
        });

        await context.UsingTenantScopeAsync(async scope =>
        {
            if (useManagement)
            {
                var section = scope.ServiceProvider.GetServices<ISiteSettingsSectionProvider>()
                    .Single(provider => provider.Descriptor.Name == "user-external-login");
                var result = await section.UpdateAsync(new JsonObject { ["useExternalProviderIfOnlyOneDefined"] = true });
                Assert.Empty(result.Errors);
                Assert.True(result.Changed);
            }
            else
            {
                var siteService = scope.ServiceProvider.GetRequiredService<ISiteService>();
                var notifier = scope.ServiceProvider.GetRequiredService<IOptionsUpdateNotifier>();

                var site = await siteService.LoadSiteSettingsAsync();
                var settings = site.GetOrCreate<ExternalLoginSettings>();
                settings.UseExternalProviderIfOnlyOneDefined = true;
                site.Put(settings);

                notifier.RequestUpdate<ExternalLoginOptions>();

                await siteService.UpdateSiteSettingsAsync(site);
            }
        });

        await context.WaitForDeferredTasksAsync(CancellationToken.None);

        await context.UsingTenantScopeAsync(scope =>
        {
            Assert.Equal(shellContextTicks, scope.ShellContext.UtcTicks);
            Assert.True(scope.ServiceProvider.GetRequiredService<IOptionsMonitor<ExternalLoginOptions>>().CurrentValue.UseExternalProviderIfOnlyOneDefined);

            return Task.CompletedTask;
        });
    }
}
