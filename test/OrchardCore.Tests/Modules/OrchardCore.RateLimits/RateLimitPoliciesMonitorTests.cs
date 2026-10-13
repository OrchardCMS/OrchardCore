using System.Text.Json.Nodes;
using OrchardCore.Entities;
using OrchardCore.RateLimits.Core;
using OrchardCore.RateLimits.Models;
using OrchardCore.RateLimits.Recipes;
using OrchardCore.RateLimits.Services;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Modules.OrchardCore.RateLimits;

public class RateLimitPoliciesMonitorTests
{
    private const string LimitedPath = "limited";

    [Fact]
    public async Task CreateEnabledPolicy_WithPolicyStore_AppliesLimitWithoutReleasingTenant()
    {
        using var context = await CreateContextAsync();

        await AssertNotLimitedAsync(context);

        var shellTicks = await context.GetShellTicksAsync();

        await context.UsingTenantScopeAsync(scope =>
            scope.ServiceProvider.GetRequiredService<IRateLimitPolicyStore>().CreateAsync(CreateEndpointPolicy()).AsTask());

        await context.WaitForDeferredTasksAsync(CancellationToken.None);

        Assert.Equal(shellTicks, await context.GetShellTicksAsync());
        await AssertLimitedAsync(context);
    }

    [Fact]
    public async Task DisablePolicy_WithPolicyStore_RemovesLimitWithoutReleasingTenant()
    {
        using var context = await CreateContextAsync();

        var policy = CreateEndpointPolicy();

        await context.UsingTenantScopeAsync(scope =>
            scope.ServiceProvider.GetRequiredService<IRateLimitPolicyStore>().CreateAsync(policy).AsTask());

        // Make sure that the policy is applied before disabling it, regardless of the update notifications.
        await context.ReleaseShellAsync();

        await AssertLimitedAsync(context);

        var shellTicks = await context.GetShellTicksAsync();

        await context.UsingTenantScopeAsync(async scope =>
            Assert.True(await scope.ServiceProvider.GetRequiredService<IRateLimitPolicyStore>().SetStatusAsync(policy.PolicyId, false)));

        await context.WaitForDeferredTasksAsync(CancellationToken.None);

        Assert.Equal(shellTicks, await context.GetShellTicksAsync());
        await AssertNotLimitedAsync(context);
    }

    [Fact]
    public async Task ImportEnabledPolicy_WithRecipeStep_AppliesLimitWithoutReleasingTenant()
    {
        using var context = await CreateContextAsync();

        await AssertNotLimitedAsync(context);

        var shellTicks = await context.GetShellTicksAsync();

        await context.ExecuteRecipeStepAsync(new JsonObject
        {
            ["name"] = CreateOrUpdateRateLimitPoliciesStep.StepKey,
            ["policies"] = new JsonArray(JObject.FromObject(CreateEndpointPolicy())),
        });

        Assert.Equal(shellTicks, await context.GetShellTicksAsync());
        await AssertLimitedAsync(context);
    }

    private static async Task<SiteContext> CreateContextAsync()
    {
        var context = new SiteContext()
            .WithRecipe("SaaS");

        await context.InitializeAsync();
        await context.EnableFeaturesAsync("OrchardCore.RateLimits");

        return context;
    }

    private static RateLimitPolicy CreateEndpointPolicy()
    {
        var limiter = new RateLimitLimiter
        {
            Id = IdGenerator.GenerateId(),
            Source = FixedWindowRateLimiterSource.SourceName,
        };

        limiter.Put(new FixedWindowRateLimiterData
        {
            PermitLimit = 1,
            QueueLimit = 0,
            WindowSeconds = 3600,
        });

        return new RateLimitPolicy
        {
            PolicyId = IdGenerator.GenerateId(),
            Name = "Limited",
            IsEnabled = true,
            Scope = RateLimitPolicyScope.Endpoint,
            Path = "/" + LimitedPath,
            Limiters = [limiter],
        };
    }

    private static async Task AssertLimitedAsync(SiteContext context)
    {
        await context.Client.GetAsync(LimitedPath, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.TooManyRequests, (await context.Client.GetAsync(LimitedPath, TestContext.Current.CancellationToken)).StatusCode);
    }

    private static async Task AssertNotLimitedAsync(SiteContext context)
    {
        for (var i = 0; i < 2; i++)
        {
            Assert.NotEqual(HttpStatusCode.TooManyRequests, (await context.Client.GetAsync(LimitedPath, TestContext.Current.CancellationToken)).StatusCode);
        }
    }
}
