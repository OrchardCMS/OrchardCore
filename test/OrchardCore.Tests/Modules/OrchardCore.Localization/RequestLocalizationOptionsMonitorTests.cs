using OrchardCore.Entities;
using OrchardCore.Environment.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Localization.Models;
using OrchardCore.Settings;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Modules.OrchardCore.Localization;

public class RequestLocalizationOptionsMonitorTests
{
    private const string NewDefaultCulture = "fr-FR";
    private const string NewSupportedCulture = "es-ES";

    [Fact]
    public async Task UpdateLocalizationSettings_WithOptionsUpdateRequest_AppliesCulturesWithoutReleasingTenant()
    {
        using var context = new SiteContext();

        await context.InitializeAsync();

        await context.UsingTenantScopeAsync(async scope =>
        {
            var shellFeaturesManager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            var availableFeatures = await shellFeaturesManager.GetAvailableFeaturesAsync();
            var featuresToEnable = availableFeatures.Where(feature => feature.Id == "OrchardCore.Localization.ContentLanguageHeader");

            await shellFeaturesManager.EnableFeaturesAsync(featuresToEnable, force: true);
        });

        await context.WaitForDeferredTasksAsync(CancellationToken.None);

        // The requested culture is not supported yet, so the response uses the current default culture.
        Assert.NotEqual(NewSupportedCulture, await GetContentLanguageAsync(context, "?culture=" + NewSupportedCulture));

        long shellContextTicks = 0;
        var fallBackToParentCultures = false;

        await context.UsingTenantScopeAsync(scope =>
        {
            shellContextTicks = scope.ShellContext.UtcTicks;
            fallBackToParentCultures = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<RequestLocalizationOptions>>().CurrentValue.FallBackToParentUICultures;

            return Task.CompletedTask;
        });

        await context.UsingTenantScopeAsync(async scope =>
        {
            var siteService = scope.ServiceProvider.GetRequiredService<ISiteService>();
            var notifier = scope.ServiceProvider.GetRequiredService<IOptionsUpdateNotifier>();

            var site = await siteService.LoadSiteSettingsAsync();
            var settings = site.GetOrCreate<LocalizationSettings>();
            settings.DefaultCulture = NewDefaultCulture;
            settings.SupportedCultures = [NewDefaultCulture, NewSupportedCulture];
            settings.FallBackToParentCulture = !fallBackToParentCultures;
            site.Put(settings);

            notifier.RequestUpdate<RequestLocalizationOptions>();

            await siteService.UpdateSiteSettingsAsync(site);
        });

        await context.WaitForDeferredTasksAsync(CancellationToken.None);

        Assert.Equal(NewDefaultCulture, await GetContentLanguageAsync(context, string.Empty));
        Assert.Equal(NewSupportedCulture, await GetContentLanguageAsync(context, "?culture=" + NewSupportedCulture));

        await context.UsingTenantScopeAsync(scope =>
        {
            Assert.Equal(shellContextTicks, scope.ShellContext.UtcTicks);

            var options = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<RequestLocalizationOptions>>().CurrentValue;

            Assert.Equal(NewDefaultCulture, options.DefaultRequestCulture.Culture.Name);
            Assert.Equal([NewDefaultCulture, NewSupportedCulture], options.SupportedCultures.Select(culture => culture.Name));
            Assert.Equal([NewDefaultCulture, NewSupportedCulture], options.SupportedUICultures.Select(culture => culture.Name));
            Assert.Equal(!fallBackToParentCultures, options.FallBackToParentCultures);
            Assert.Equal(!fallBackToParentCultures, options.FallBackToParentUICultures);

            return Task.CompletedTask;
        });
    }

    private static async Task<string> GetContentLanguageAsync(SiteContext context, string requestUri)
    {
        using var response = await context.Client.GetAsync(requestUri);

        return string.Join(",", response.Content.Headers.ContentLanguage);
    }
}
