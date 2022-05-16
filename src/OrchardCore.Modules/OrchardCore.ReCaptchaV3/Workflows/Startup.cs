using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using OrchardCore.Workflows.Helpers;

namespace OrchardCore.ReCaptchaV3.Workflows
{
    [RequireFeatures("OrchardCore.Workflows", "OrchardCore.ReCaptchaV3")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddActivity<ValidateReCaptchaV3Task, ValidateReCaptchaV3TaskDisplayDriver>();
        }
    }
}
