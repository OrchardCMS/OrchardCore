using Fluid;
using Fluid.Values;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Liquid;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Queries.Core.Services;
using OrchardCore.Queries.Deployment;
using OrchardCore.Queries.Drivers;
using OrchardCore.Queries.Liquid;
using OrchardCore.Queries.Recipes;
using OrchardCore.Recipes;
using OrchardCore.Scripting;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Queries;

/// <summary>
/// These services are registered on the tenant service collection.
/// </summary>
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddNavigationProvider<AdminMenu>();
        services.AddDisplayDriver<Query, QueryDisplayDriver>();
        services.AddPermissionProvider<Permissions>();
    }
}

[Feature("OrchardCore.Queries.Core")]
public sealed class CoreStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddRecipeExecutionStep<QueryStep>();
        services.AddDeployment<AllQueriesDeploymentSource, AllQueriesDeploymentStep, AllQueriesDeploymentStepDriver>();
        services.AddSingleton<IGlobalMethodProvider, QueryGlobalMethodProvider>();

        services.Configure<TemplateOptions>(o =>
        {
            o.Scope.SetValue("Queries", new ObjectValue(new LiquidQueriesAccessor()));
            o.MemberAccessStrategy.Register<LiquidQueriesAccessor, FluidValue>(async (obj, name, context) =>
            {
                var liquidTemplateContext = (LiquidTemplateContext)context;
                var queryManager = liquidTemplateContext.Services.GetRequiredService<IQueryManager>();

                var query = await queryManager.GetQueryAsync(name);

                return FluidValue.Create(query, context.Options);
            });
        })
        .AddLiquidFilter<QueryFilter>("query");

        services.AddScoped<IQueryManager, DefaultQueryManager>();
    }
}

[RequireFeatures("OrchardCore.Deployment", "OrchardCore.Contents")]
public class DeploymentStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDeployment<QueryBasedContentDeploymentSource, QueryBasedContentDeploymentStep, QueryBasedContentDeploymentStepDriver>();
    }
}
