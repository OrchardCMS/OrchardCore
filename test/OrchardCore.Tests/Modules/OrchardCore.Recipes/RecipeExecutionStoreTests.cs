using Microsoft.Extensions.Logging.Abstractions;
using OrchardCore.Modules;
using OrchardCore.Recipes.Services;
using OrchardCore.Tests.Apis.Context;
using YesSql;

namespace OrchardCore.Tests.Modules.OrchardCore.Recipes;

public class RecipeExecutionStoreTests
{
    [Fact]
    public async Task Execution_FreshScopes_KeepsItsStateAndIsNotStartedTwice()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        var now = DateTime.UtcNow;

        await context.UsingTenantScopeAsync(async scope =>
        {
            var store = Store(scope.ServiceProvider, () => now);
            Assert.Null(await store.FindAsync("request"));

            var (started, execution) = await store.TryStartAsync("request", "recipe", "Recipe");

            Assert.True(started);
            Assert.Equal(RecipeExecutionState.Running, execution.State);
        });

        await context.UsingTenantScopeAsync(async scope =>
        {
            var store = Store(scope.ServiceProvider, () => now);
            var (started, execution) = await store.TryStartAsync("request", "other", "Other");

            Assert.False(started);
            Assert.Equal("recipe", execution.RecipeId);

            await store.CompleteAsync("request", RecipeExecutionState.Failed, ["Step failed."]);
            await store.CompleteAsync("unknown", RecipeExecutionState.Succeeded);
        });

        await context.UsingTenantScopeAsync(async scope =>
        {
            var execution = await Store(scope.ServiceProvider, () => now).FindAsync("request");

            Assert.Equal(RecipeExecutionState.Failed, execution.State);
            Assert.Equal(["Step failed."], execution.Errors);
            Assert.Equal("Recipe", execution.RecipeName);
            Assert.NotNull(execution.CompletedUtc);
        });
    }

    [Fact]
    public async Task Execution_NoOutcomeBeforeTheDeadline_IsReportedUncertainUntilItCompletes()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        var now = DateTime.UtcNow;

        await context.UsingTenantScopeAsync(async scope =>
        {
            var store = Store(scope.ServiceProvider, () => now);
            await store.TryStartAsync("request", "recipe", "Recipe");
            Assert.Equal(RecipeExecutionState.Running, (await store.FindAsync("request")).State);

            now += RecipeExecutionStore.OutcomeDeadline;
            Assert.Equal(RecipeExecutionState.Uncertain, (await store.FindAsync("request")).State);
            Assert.Equal(RecipeExecutionState.Uncertain, (await store.TryStartAsync("request", "recipe", "Recipe")).Execution.State);

            await store.CompleteAsync("request", RecipeExecutionState.Succeeded);
            Assert.Equal(RecipeExecutionState.Succeeded, (await store.FindAsync("request")).State);
        });
    }

    [Fact]
    public async Task Log_BeyondItsLimit_RemovesTheOldestEndedExecutions()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        var now = DateTime.UtcNow;

        await context.UsingTenantScopeAsync(async scope =>
        {
            var store = Store(scope.ServiceProvider, () => now);
            await store.TryStartAsync("running", "recipe", "Recipe");

            for (var i = 0; i < RecipeExecutionStore.MaxExecutions; i++)
            {
                now += TimeSpan.FromSeconds(1);
                await store.TryStartAsync("ended" + i, "recipe", "Recipe");
                await store.CompleteAsync("ended" + i, RecipeExecutionState.Succeeded);
            }

            Assert.Null(await store.FindAsync("ended0"));
            Assert.NotNull(await store.FindAsync("ended1"));
            Assert.NotNull(await store.FindAsync("running"));
        });
    }

    private static RecipeExecutionStore Store(IServiceProvider services, Func<DateTime> now)
    {
        var clock = new Mock<IClock>();
        clock.SetupGet(value => value.UtcNow).Returns(now);

        return new RecipeExecutionStore(services.GetRequiredService<IStore>(), clock.Object, NullLogger<RecipeExecutionStore>.Instance);
    }
}
