using Microsoft.AspNetCore.Modules;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Data.Migration;
using Orchard.Environment.Navigation;
using Orchard.Security.Permissions;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
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

            services.AddScoped<IIndexProvider, ActivityIndexProvider>();
            services.AddScoped<IIndexProvider, AwaitingActivityIndexProvider>();
            services.AddScoped<IIndexProvider, WorkflowDefinitionIndexProvider>();
            services.AddScoped<IIndexProvider, WorkflowWorkflowDefinitionIndexProvider>();

            services.AddScoped<IActivity, DeleteActivity>();
            services.AddScoped<IActivity, NotificationActivity>();
            services.AddScoped<IActivity, ContentCreatedActivity>();
            services.AddScoped<IActivity, ContentUpdatedActivity>();
            services.AddScoped<IActivity, ContentPublishedActivity>();
            services.AddScoped<IActivity, ContentVersionedActivity>();
            services.AddScoped<IActivity, ContentRemovedActivity>();

            services.AddScoped<IDataMigration, Migrations>();

            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IPermissionProvider, Permissions>();
        }
    }
}
