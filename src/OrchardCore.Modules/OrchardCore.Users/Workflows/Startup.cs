using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Users.Workflows.Activities;
using OrchardCore.Users.Workflows.Drivers;
using OrchardCore.Modules;
using OrchardCore.Workflows.Helpers;

namespace OrchardCore.Users.Workflows
{
    [RequireFeatures("OrchardCore.Workflows")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddActivity<RegisterUserTask, RegisterUserTaskDisplay>();
        }
    }
}
