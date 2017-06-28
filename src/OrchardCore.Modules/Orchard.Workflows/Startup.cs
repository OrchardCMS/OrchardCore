using Microsoft.AspNetCore.Modules;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Data.Migration;
using Orchard.Environment.Navigation;
using Orchard.Security.Permissions;
using Orchard.Workflows.Indexes;
using Orchard.Workflows.Services;
using YesSql.Indexes;

namespace Orchard.Workflows
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IActivitiesManager, ActivitiesManager>();
            services.AddScoped<IWorkflowManager, WorkflowManager>();

            services.AddScoped<IIndexProvider, ActivityIndexProvider>();
            services.AddScoped<IIndexProvider, AwaitingActivityndexProvider>();
            services.AddScoped<IIndexProvider, WorkflowDefinitionIndexProvider>();
            services.AddScoped<IDataMigration, Migrations>();

            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IPermissionProvider, Permissions>();
        }
    }
}
