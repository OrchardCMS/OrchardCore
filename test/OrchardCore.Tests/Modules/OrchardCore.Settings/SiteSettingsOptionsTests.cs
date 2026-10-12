using OrchardCore.Environment.Cache;
using OrchardCore.Environment.Options;
using OrchardCore.Navigation;
using OrchardCore.ResourceManagement;
using OrchardCore.Settings;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Modules.OrchardCore.Settings;

public class SiteSettingsOptionsTests
{
    [Fact]
    public async Task UseCdn_Updated_RendersCdnUrlsWithoutReleasingTenant()
    {
        using var context = new SiteContext()
            .WithRecipe("SaaS");

        await context.InitializeAsync();

        var shellContextTicks = await GetShellContextTicksAsync(context);

        Assert.DoesNotContain("https://", await RenderJQueryAsync(context));

        await UpdateSiteAsync(context, site => site.UseCdn = true, notifier => notifier
            .RequestUpdate<ResourceOptions>()
            .RequestUpdate<ResourceManagementOptions>());

        Assert.Contains("https://", await RenderJQueryAsync(context));
        Assert.Equal(shellContextTicks, await GetShellContextTicksAsync(context));
    }

    [Fact]
    public async Task PageSize_Updated_AppliesWithoutReleasingTenant()
    {
        using var context = new SiteContext()
            .WithRecipe("SaaS");

        await context.InitializeAsync();

        var shellContextTicks = await GetShellContextTicksAsync(context);

        await context.UsingTenantScopeAsync(scope =>
        {
            Assert.NotEqual(7, scope.ServiceProvider.GetRequiredService<IOptionsMonitor<PagerOptions>>().CurrentValue.GetPageSize());

            return Task.CompletedTask;
        });

        await UpdateSiteAsync(context, site => site.PageSize = 7, notifier => notifier.RequestUpdate<PagerOptions>());

        await context.UsingTenantScopeAsync(scope =>
        {
            Assert.Equal(7, scope.ServiceProvider.GetRequiredService<IOptionsMonitor<PagerOptions>>().CurrentValue.GetPageSize());

            return Task.CompletedTask;
        });

        Assert.Equal(shellContextTicks, await GetShellContextTicksAsync(context));
    }

    [Fact]
    public async Task CacheMode_Updated_AppliesWithoutReleasingTenant()
    {
        using var context = new SiteContext()
            .WithRecipe("SaaS");

        await context.InitializeAsync();

        var shellContextTicks = await GetShellContextTicksAsync(context);

        await context.UsingTenantScopeAsync(scope =>
        {
            Assert.False(scope.ServiceProvider.GetRequiredService<IOptionsMonitor<CacheOptions>>().CurrentValue.DebugMode);

            return Task.CompletedTask;
        });

        await UpdateSiteAsync(context, site => site.CacheMode = CacheMode.DebugEnabled, notifier => notifier.RequestUpdate<CacheOptions>());

        await context.UsingTenantScopeAsync(scope =>
        {
            var cacheOptions = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<CacheOptions>>().CurrentValue;

            Assert.True(cacheOptions.Enabled);
            Assert.True(cacheOptions.DebugMode);

            return Task.CompletedTask;
        });

        Assert.Equal(shellContextTicks, await GetShellContextTicksAsync(context));
    }

    private static async Task<string> RenderJQueryAsync(SiteContext context)
    {
        var html = string.Empty;

        await context.UsingTenantScopeAsync(scope =>
        {
            var resourceManager = scope.ServiceProvider.GetRequiredService<IResourceManager>();
            resourceManager.RegisterResource("script", "jQuery").AtFoot();

            using var writer = new StringWriter();
            resourceManager.RenderFootScript(writer);
            html = writer.ToString();

            return Task.CompletedTask;
        });

        return html;
    }

    private static async Task UpdateSiteAsync(SiteContext context, Action<ISite> update, Action<IOptionsUpdateNotifier> requestUpdates)
    {
        await context.UsingTenantScopeAsync(async scope =>
        {
            var siteService = scope.ServiceProvider.GetRequiredService<ISiteService>();

            var site = await siteService.LoadSiteSettingsAsync();
            update(site);

            requestUpdates(scope.ServiceProvider.GetRequiredService<IOptionsUpdateNotifier>());

            await siteService.UpdateSiteSettingsAsync(site);
        });

        await context.WaitForDeferredTasksAsync(CancellationToken.None);
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
