using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using OrchardCore.Workflows.Helpers;

namespace OrchardCore.ReCaptcha.Workflows
{
    [RequireFeatures("OrchardCore.Workflows", "OrchardCore.ReCaptcha")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddActivity<ValidateReCaptchaTask, ValidateReCaptchaTaskDisplayDriver>();
        }
    }
}
