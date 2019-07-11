using OrchardCore.Modules;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Navigation;
using OrchardCore.Liquid;
using OrchardCore.Queries.Deployment;
using OrchardCore.Queries.Drivers;
using OrchardCore.Queries.Liquid;
using OrchardCore.Queries.Recipes;
using OrchardCore.Queries.Services;
using OrchardCore.Recipes;
using OrchardCore.Security.Permissions;
using OrchardCore.Scripting;

namespace OrchardCore.Queries
{
    /// <summary>
    /// These services are registered on the tenant service collection
    /// </summary>
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IQueryManager, QueryManager>();
            services.AddScoped<IDisplayManager<Query>, DisplayManager<Query>>();

            services.AddScoped<IDisplayDriver<Query>, QueryDisplayDriver>();
            services.AddRecipeExecutionStep<QueryStep>();
            services.AddScoped<IPermissionProvider, Permissions>();


            services.AddTransient<IDeploymentSource, AllQueriesDeploymentSource>();
            services.AddSingleton<IDeploymentStepFactory>(new DeploymentStepFactory<AllQueriesDeploymentStep>());
            services.AddScoped<IDisplayDriver<DeploymentStep>, AllQueriesDeploymentStepDriver>();
            services.AddSingleton<IGlobalMethodProvider, QueryGlobalMethodProvider>();
        }
    }


    [RequireFeatures("OrchardCore.Liquid")]
    public class LiquidStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<ILiquidTemplateEventHandler, QueriesLiquidTemplateEventHandler>();

            services.AddLiquidFilter<QueryFilter>("query");
        }
    }
}
