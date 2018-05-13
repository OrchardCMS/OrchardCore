using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Forms.Workflows.Activities;
using OrchardCore.Forms.Workflows.Drivers;
using OrchardCore.Modules;
using OrchardCore.Workflows.Helpers;

namespace OrchardCore.Forms.Workflows
{
    [RequireFeatures("OrchardCore.Workflows")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddActivity<ValidateAntiforgeryTokenTask, ValidateAntiforgeryTokenTaskDisplay>();
            services.AddAntiforgery();
        }
    }
}
