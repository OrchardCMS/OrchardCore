using System.Text.Json.Nodes;
using OrchardCore.Navigation;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Recipes;

public class RecipeShellReleaseTests
{
    [Fact]
    public async Task ImportDeploymentPackage_SiteSettings_AppliesWithoutReleasingTenant()
    {
        // The default recipe enables the deployment feature used to import recipes.
        using var context = new SiteContext();

        await context.InitializeAsync();

        await context.UsingTenantScopeAsync(scope =>
        {
            Assert.NotEqual(9, scope.ServiceProvider.GetRequiredService<IOptionsMonitor<PagerOptions>>().CurrentValue.GetPageSize());

            return Task.CompletedTask;
        });

        var shellContextTicks = await GetShellContextTicksAsync(context);

        await ImportAsync(context, new JsonObject
        {
            ["name"] = "Settings",
            ["PageSize"] = 9,
            ["LoginSettings"] = new JsonObject
            {
                ["DisableLocalLogin"] = false,
            },
        });

        await context.UsingTenantScopeAsync(scope =>
        {
            Assert.Equal(9, scope.ServiceProvider.GetRequiredService<IOptionsMonitor<PagerOptions>>().CurrentValue.GetPageSize());

            return Task.CompletedTask;
        });

        Assert.Equal(shellContextTicks, await GetShellContextTicksAsync(context));
    }

    [Fact]
    public async Task ImportDeploymentPackage_SettingsRequiringRelease_ReleasesTenant()
    {
        // The default recipe enables the deployment feature used to import recipes.
        using var context = new SiteContext();

        await context.InitializeAsync();

        var shellContextTicks = await GetShellContextTicksAsync(context);

        await ImportAsync(context, new JsonObject
        {
            ["name"] = "Settings",
            ["IdentitySettings"] = new JsonObject
            {
                ["UserSettings"] = new JsonObject
                {
                    ["AllowedUserNameCharacters"] = "abcdefghijklmnopqrstuvwxyz",
                    ["RequireUniqueEmail"] = true,
                },
            },
        });

        Assert.NotEqual(shellContextTicks, await GetShellContextTicksAsync(context));

        await context.UsingTenantScopeAsync(scope =>
        {
            Assert.Equal("abcdefghijklmnopqrstuvwxyz", scope.ServiceProvider.GetRequiredService<IOptions<IdentityOptions>>().Value.User.AllowedUserNameCharacters);

            return Task.CompletedTask;
        });
    }

    [Fact]
    public async Task ImportDeploymentPackage_ReloadTenantStep_ReleasesTenant()
    {
        // The default recipe enables the deployment feature used to import recipes.
        using var context = new SiteContext();

        await context.InitializeAsync();

        var shellContextTicks = await GetShellContextTicksAsync(context);

        await ImportAsync(context, new JsonObject
        {
            ["name"] = "ReloadTenant",
        });

        Assert.NotEqual(shellContextTicks, await GetShellContextTicksAsync(context));
    }

    private static async Task ImportAsync(SiteContext context, JsonObject step)
    {
        await RecipeHelpers.RunRecipeAsync(context, new JsonObject
        {
            ["name"] = "Test",
            ["steps"] = new JsonArray(step),
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
