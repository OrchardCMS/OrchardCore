using Microsoft.AspNetCore.Authentication;
using OrchardCore.Environment.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.OptionsMonitoring;

/// <summary>
/// Helpers to check that updating the settings of a tenant refreshes its options without releasing it.
/// </summary>
internal static class TenantOptionsTestHelper
{
    public static async Task<SiteContext> CreateSiteWithFeaturesAsync(params string[] featureIds)
    {
        var context = new SiteContext()
            .WithRecipe("SaaS");

        await context.InitializeAsync();

        await EnableFeaturesAsync(context, featureIds);

        return context;
    }

    public static async Task EnableFeaturesAsync(SiteContext context, params string[] featureIds)
    {
        await context.UsingTenantScopeAsync(async scope =>
        {
            var shellFeaturesManager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            var availableFeatures = await shellFeaturesManager.GetAvailableFeaturesAsync();
            var featuresToEnable = availableFeatures.Where(feature => featureIds.Contains(feature.Id)).ToArray();

            Assert.Equal(featureIds.Length, featuresToEnable.Length);

            await shellFeaturesManager.EnableFeaturesAsync(featuresToEnable, force: true);
        });

        await context.WaitForDeferredTasksAsync(CancellationToken.None);
    }

    public static async Task<long> GetShellContextTicksAsync(SiteContext context)
    {
        long ticks = 0;

        await context.UsingTenantScopeAsync(scope =>
        {
            ticks = scope.ShellContext.UtcTicks;

            return Task.CompletedTask;
        });

        return ticks;
    }

    /// <summary>
    /// Updates the site settings and requests the options updates in the same scope, as the settings drivers do.
    /// </summary>
    public static async Task UpdateSiteAsync(SiteContext context, Action<ISite, IServiceProvider> update, Action<IOptionsUpdateNotifier> requestUpdates)
    {
        await context.UsingTenantScopeAsync(async scope =>
        {
            var siteService = scope.ServiceProvider.GetRequiredService<ISiteService>();

            var site = await siteService.LoadSiteSettingsAsync();
            update(site, scope.ServiceProvider);

            requestUpdates(scope.ServiceProvider.GetRequiredService<IOptionsUpdateNotifier>());

            await siteService.UpdateSiteSettingsAsync(site);
        });

        await context.WaitForDeferredTasksAsync(CancellationToken.None);
    }

    public static async Task<T> GetAsync<T>(SiteContext context, Func<IServiceProvider, Task<T>> get)
    {
        T result = default;

        await context.UsingTenantScopeAsync(async scope => result = await get(scope.ServiceProvider));

        return result;
    }

    public static Task<T> GetAsync<T>(SiteContext context, Func<IServiceProvider, T> get)
        => GetAsync(context, serviceProvider => Task.FromResult(get(serviceProvider)));

    public static Task<AuthenticationScheme> GetSchemeAsync(SiteContext context, string name)
        => GetAsync(context, serviceProvider => serviceProvider.GetRequiredService<IAuthenticationSchemeProvider>().GetSchemeAsync(name));

    /// <summary>
    /// Challenges the specified scheme and returns the URL the tenant redirects to.
    /// </summary>
    public static async Task<string> ChallengeAsync(SiteContext context, string scheme)
    {
        string location = null;

        await context.UsingTenantScopeAsync(async scope =>
        {
            var httpContext = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext;

            await httpContext.ChallengeAsync(scheme, new AuthenticationProperties { RedirectUri = "/" });

            location = httpContext.Response.Headers.Location;
        });

        return location;
    }
}
