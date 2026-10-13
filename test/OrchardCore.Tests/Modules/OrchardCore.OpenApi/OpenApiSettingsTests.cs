using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.OpenApi.Settings;
using OrchardCore.Settings;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Modules.OrchardCore.OpenApi;

public class OpenApiSettingsTests
{
    [Fact]
    public async Task AllowAnonymousSchemaAccess_Enabled_AppliesWithoutReleasingTenant()
    {
        using var context = new SiteContext()
            .WithRecipe("SaaS");

        await context.InitializeAsync();

        await context.UsingTenantScopeAsync(async scope =>
        {
            var shellFeaturesManager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            var availableFeatures = await shellFeaturesManager.GetAvailableFeaturesAsync();

            await shellFeaturesManager.EnableFeaturesAsync(availableFeatures.Where(feature => feature.Id == "OrchardCore.OpenApi"), force: true);
        });

        await context.WaitForDeferredTasksAsync(CancellationToken.None);

        var response = await context.Client.GetAsync("openapi/v1.json", TestContext.Current.CancellationToken);
        Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);

        var shellContextTicks = await GetShellContextTicksAsync(context);

        await context.UsingTenantScopeAsync(async scope =>
        {
            var siteService = scope.ServiceProvider.GetRequiredService<ISiteService>();

            var site = await siteService.LoadSiteSettingsAsync();
            site.Alter<OpenApiSettings>(settings => settings.AllowAnonymousSchemaAccess = true);

            await siteService.UpdateSiteSettingsAsync(site);
        });

        await context.WaitForDeferredTasksAsync(CancellationToken.None);

        response = await context.Client.GetAsync("openapi/v1.json", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Assert.Equal(shellContextTicks, await GetShellContextTicksAsync(context));
    }

    private static async Task<long> GetShellContextTicksAsync(SiteContext context)
    {
        long ticks = 0;

        await context.UsingTenantScopeAsync(scope =>
        {
            ticks = scope.ShellContext.UtcTicks;

            return Task.CompletedTask;
        });

        return ticks;
    }
}
