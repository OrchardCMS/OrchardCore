using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using OrchardCore.Twitter.Workflows.Activities;
using OrchardCore.Twitter.Workflows.Drivers;
using OrchardCore.Workflows.Helpers;

namespace OrchardCore.Twitter.Workflows
{
    [RequireFeatures("OrchardCore.Workflows")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddActivity<UpdateTwitterStatusTask, UpdateTwitterStatusTaskDisplayDriver>();
        }
    }
}
