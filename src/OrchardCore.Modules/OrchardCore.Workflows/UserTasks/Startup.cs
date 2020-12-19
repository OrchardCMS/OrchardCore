using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.Modules;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.UserTasks.Activities;
using OrchardCore.Workflows.UserTasks.Drivers;

namespace OrchardCore.Workflows.UserTasks
{
    [RequireFeatures("OrchardCore.Workflows", "OrchardCore.Contents", "OrchardCore.Roles")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IContentDisplayDriver, UserTaskEventContentDriver>();
            services.AddActivity<UserTaskEvent, UserTaskEventDisplayDriver>();
        }
    }
}
