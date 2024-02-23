using Fluid;
using Fluid.Values;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Liquid;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Queries.Deployment;
using OrchardCore.Queries.Drivers;
using OrchardCore.Queries.Liquid;
using OrchardCore.Queries.Recipes;
using OrchardCore.Queries.Services;
using OrchardCore.Recipes;
using OrchardCore.Scripting;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Queries
{
    /// <summary>
    /// These services are registered on the tenant service collection.
    /// </summary>
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IQueryManager, QueryManager>();
            services.AddScoped<IDisplayDriver<Query>, QueryDisplayDriver>();
            services.AddRecipeExecutionStep<QueryStep>();
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddDeployment<AllQueriesDeploymentSource, AllQueriesDeploymentStep, AllQueriesDeploymentStepDriver>();
            services.AddSingleton<IGlobalMethodProvider, QueryGlobalMethodProvider>();

            services.Configure<TemplateOptions>(o =>
            {
                o.Scope.SetValue("Queries", new ObjectValue(new LiquidQueriesAccessor()));
                o.MemberAccessStrategy.Register<LiquidQueriesAccessor, FluidValue>(async (obj, name, context) =>
                {
                    var liquidTemplateContext = (LiquidTemplateContext)context;
                    var queryManager = liquidTemplateContext.Services.GetRequiredService<IQueryManager>();

                    return FluidValue.Create(await queryManager.GetQueryAsync(name), context.Options);
                });
            })
            .AddLiquidFilter<QueryFilter>("query");
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
}
