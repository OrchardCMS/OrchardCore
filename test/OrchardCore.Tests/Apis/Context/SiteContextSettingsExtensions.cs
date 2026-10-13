using System.Text.Json.Nodes;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.Environment.Shell;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Settings;
using OrchardCore.Tests.Stubs;

namespace OrchardCore.Tests.Apis.Context;

internal static class SiteContextSettingsExtensions
{
    /// <summary>
    /// Enables the specified features on the tenant and waits for the tenant to be reloaded.
    /// </summary>
    public static async Task EnableFeaturesAsync(this SiteContext context, params string[] featureIds)
    {
        await context.UsingTenantScopeAsync(async scope =>
        {
            var shellFeaturesManager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            var availableFeatures = await shellFeaturesManager.GetAvailableFeaturesAsync();

            await shellFeaturesManager.EnableFeaturesAsync(availableFeatures.Where(feature => featureIds.Contains(feature.Id)), force: true);
        });

        await context.WaitForDeferredTasksAsync(CancellationToken.None);
    }

    /// <summary>
    /// Gets the creation ticks of the tenant shell, which change whenever the shell is released.
    /// </summary>
    public static async Task<long> GetShellTicksAsync(this SiteContext context)
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
    /// Releases the tenant shell so that it is rebuilt on the next request.
    /// </summary>
    public static async Task ReleaseShellAsync(this SiteContext context)
    {
        Assert.True(SiteContext.ShellHost.TryGetSettings(context.TenantName, out var shellSettings));

        await SiteContext.ShellHost.ReleaseShellContextAsync(shellSettings);

        await context.WaitForDeferredTasksAsync(CancellationToken.None);
    }

    /// <summary>
    /// Executes a single recipe step on the tenant, as the recipe executor does, then waits for the deferred tasks
    /// of the scope to complete.
    /// </summary>
    public static async Task ExecuteRecipeStepAsync(this SiteContext context, JsonObject step)
    {
        await context.UsingTenantScopeAsync(async scope =>
        {
            var recipeContext = new RecipeExecutionContext
            {
                ExecutionId = Guid.NewGuid().ToString("n"),
                Name = step["name"]?.GetValue<string>(),
                Step = step,
            };

            foreach (var handler in scope.ServiceProvider.GetServices<IRecipeStepHandler>())
            {
                await handler.ExecuteAsync(recipeContext);
            }

            Assert.Empty(recipeContext.Errors);
        });

        await context.WaitForDeferredTasksAsync(CancellationToken.None);
    }

    /// <summary>
    /// Saves a site settings group through its display driver, as the admin settings page does,
    /// then waits for the deferred tasks of the scope to complete.
    /// </summary>
    public static async Task UpdateSiteSettingsWithDriverAsync<TDriver>(
        this SiteContext context,
        string groupId,
        Action<object> bind,
        Action<HttpContext> configureHttpContext = null)
        where TDriver : IDisplayDriver<ISite>
    {
        await context.UsingTenantScopeAsync(async scope =>
        {
            configureHttpContext?.Invoke(scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext);

            var siteService = scope.ServiceProvider.GetRequiredService<ISiteService>();
            var driver = scope.ServiceProvider.GetServices<IDisplayDriver<ISite>>().OfType<TDriver>().Single();

            var site = await siteService.LoadSiteSettingsAsync();
            var updater = new DelegateModelUpdater(bind);

            var result = await driver.UpdateEditorAsync(site, new UpdateEditorContext(new Shape(), groupId, false, string.Empty, null, null, updater));

            Assert.NotNull(result);
            Assert.True(updater.ModelState.IsValid);

            await siteService.UpdateSiteSettingsAsync(site);
        });

        await context.WaitForDeferredTasksAsync(CancellationToken.None);
    }
}
