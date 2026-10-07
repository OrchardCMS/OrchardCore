using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Data.Migration;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.RateLimits.Core;
using OrchardCore.RateLimits.Deployment;
using OrchardCore.RateLimits.Drivers;
using OrchardCore.RateLimits.Migrations;
using OrchardCore.RateLimits.Models;
using OrchardCore.RateLimits.Recipes;
using OrchardCore.RateLimits.Services;
using OrchardCore.Recipes;
using OrchardCore.Security.Permissions;

namespace OrchardCore.RateLimits;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddRateLimiter();
        services.AddDataMigration<GlobalRateLimitsMigrations>();
        services.AddTransient<IConfigureOptions<RateLimiterOptions>, RateLimiterOptionsConfigurations>();
        services.AddSingleton<IRateLimitPolicyStore, RateLimitPolicyStore>();
        services.AddDisplayDriver<RateLimitLimiter, RateLimitLimiterDisplayDriver>();
        services.AddDisplayDriver<RateLimitPolicy, RateLimitPolicyDisplayDriver>();

        services.AddKeyedSingleton<IRateLimiterSource, FixedWindowRateLimiterSource>(FixedWindowRateLimiterSource.SourceName)
            .AddDisplayDriver<RateLimitLimiter, FixedWindowRateLimiterDisplayDriver>();

        services.AddKeyedSingleton<IRateLimiterSource, SlidingWindowRateLimiterSource>(SlidingWindowRateLimiterSource.SourceName)
            .AddDisplayDriver<RateLimitLimiter, SlidingWindowRateLimiterDisplayDriver>();

        services.AddKeyedSingleton<IRateLimiterSource, ConcurrencyRateLimiterSource>(ConcurrencyRateLimiterSource.SourceName)
            .AddDisplayDriver<RateLimitLimiter, ConcurrencyRateLimiterDisplayDriver>();

        services.AddKeyedSingleton<IRateLimiterSource, TokenBucketRateLimiterSource>(TokenBucketRateLimiterSource.SourceName)
            .AddDisplayDriver<RateLimitLimiter, TokenBucketRateLimiterDisplayDriver>();

        services.AddNavigationProvider<AdminMenu>();
        services.AddPermissionProvider<Permissions>();
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        app.UseRateLimiter();
    }
}

[RequireFeatures("OrchardCore.Recipes.Core")]
public sealed class RecipesStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddRecipeExecutionStep<CreateOrUpdateRateLimitPoliciesStep>();
    }
}

[RequireFeatures("OrchardCore.Deployment")]
public sealed class DeploymentStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDeployment<AllRateLimitPoliciesDeploymentSource, AllRateLimitPoliciesDeploymentStep, AllRateLimitPoliciesDeploymentStepDriver>();
    }
}
