using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundTasks;
using OrchardCore.Modules;
using OrchardCore.Workflows.Helpers;

namespace OrchardCore.Workflows.Timers;

[Feature("OrchardCore.Workflows.Timers")]
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddActivity<TimerEvent, TimerEventDisplayDriver>(activity => activity.Icon = "fa-solid fa-clock");
        services.AddSingleton<IBackgroundTask, TimerBackgroundTask>();
    }
}
