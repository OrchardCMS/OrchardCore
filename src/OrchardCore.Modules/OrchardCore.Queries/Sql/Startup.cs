using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Queries.Sql.Deployment;
using OrchardCore.Queries.Sql.Drivers;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Queries.Sql
{
    /// <summary>
    /// These services are registered on the tenant service collection
    /// </summary>
    [Feature("OrchardCore.Queries.Sql")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<IDisplayDriver<Query>, SqlQueryDisplayDriver>();
            services.AddScoped<IQuerySource, SqlQuerySource>();
            services.AddScoped<INavigationProvider, AdminMenu>();

            services.AddTransient<IDeploymentSource, QueryBasedContentDeploymentSource>();
            services.AddSingleton<IDeploymentStepFactory>(new DeploymentStepFactory<QueryBasedContentDeploymentStep>());
            services.AddScoped<IDisplayDriver<DeploymentStep>, QueryBasedContentDeploymentStepDriver>();
        }
    }
}
