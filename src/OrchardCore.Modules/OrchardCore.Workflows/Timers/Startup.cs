using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundTasks;
using OrchardCore.Modules;
using OrchardCore.Workflows.Helpers;

namespace OrchardCore.Workflows.Timers
{
    [Feature("OrchardCore.Workflows.Timers")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddActivity<TimerEvent, TimerEventDisplayDriver>();
            services.AddSingleton<IBackgroundTask, TimerBackgroundTask>();
        }
    }
}
