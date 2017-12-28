using System;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Navigation;
using OrchardCore.Modules;
using OrchardCore.Security.Permissions;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Drivers;
using OrchardCore.Workflows.Indexes;
using OrchardCore.Workflows.Services;
using YesSql.Indexes;

namespace OrchardCore.Workflows
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped(typeof(Resolver<>));
            services.AddScoped<IActivityLibrary, ActivityLibrary>();
            services.AddScoped<IWorkflowDefinitionRepository, WorkflowDefinitionRepository>();
            services.AddScoped<IWorkflowInstanceRepository, WorkflowInstanceRepository>();
            services.AddScoped<IWorkflowManager, WorkflowManager>();
            services.AddScoped<IDisplayManager<IActivity>, DisplayManager<IActivity>>();
            services.AddScoped<IDataMigration, Migrations>();
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IPermissionProvider, Permissions>();

            services.AddSingleton<IIndexProvider, WorkflowDefinitionIndexProvider>();
            services.AddSingleton<IIndexProvider, WorkflowInstanceIndexProvider>();

            services.AddScoped<IActivity, NotifyTask>();
            services.AddScoped<IDisplayDriver<IActivity>, NotifyTaskDisplay>();
        }
    }
}
