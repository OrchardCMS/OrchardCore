using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Email.Workflows.Activities;
using OrchardCore.Email.Workflows.Drivers;
using OrchardCore.Modules;
using OrchardCore.Workflows.Helpers;

namespace OrchardCore.Email.Workflows
{
    [RequireFeatures("OrchardCore.Workflows")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddActivity<EmailTask, EmailTaskDisplayDriver>();
        }
    }
}
