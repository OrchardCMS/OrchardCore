using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Data.Migration;
using OrchardCore.Environment.Navigation;
using OrchardCore.Modules;
using OrchardCore.Security.Permissions;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Indexes;
using OrchardCore.Workflows.Services;
using YesSql.Indexes;

namespace OrchardCore.Workflows
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IActivitiesManager, ActivitiesManager>();
            services.AddScoped<IWorkflowManager, WorkflowManager>();

            services.AddScoped<IIndexProvider, WorkflowDefinitionIndexProvider>();
            services.AddScoped<IIndexProvider, WorkflowInstanceIndexProvider>();

            services.AddScoped<IActivity, Notify>();

            services.AddScoped<IDataMigration, Migrations>();

            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IPermissionProvider, Permissions>();
        }
    }
}
