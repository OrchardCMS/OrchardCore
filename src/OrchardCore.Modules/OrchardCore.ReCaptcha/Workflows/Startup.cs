using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Options;

namespace OrchardCore.ReCaptcha.Workflows;

[RequireFeatures("OrchardCore.Workflows", "OrchardCore.ReCaptcha")]
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.Configure<WorkflowOptions>(options => options.CategoryIcons[ActivityCategoryLocalizationSources.Validation.Value] = "fa-solid fa-check-double");

        services.AddActivity<ValidateReCaptchaTask, ValidateReCaptchaTaskDisplayDriver>();
    }
}
