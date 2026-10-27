using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Forms.Workflows.Activities;
using OrchardCore.Forms.Workflows.Drivers;
using OrchardCore.Modules;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Options;

namespace OrchardCore.Forms.Workflows;

[RequireFeatures("OrchardCore.Workflows")]
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.Configure<WorkflowOptions>(options => options.CategoryIcons["Validation"] = "fa-solid fa-check-double");

        services.AddActivity<ValidateAntiforgeryTokenTask, ValidateAntiforgeryTokenTaskDisplayDriver>();
        services.AddActivity<AddModelValidationErrorTask, AddModelValidationErrorTaskDisplayDriver>();
        services.AddActivity<ValidateFormTask, ValidateFormTaskDisplayDriver>();
        services.AddActivity<ValidateFormFieldTask, ValidateFormFieldTaskDisplayDriver>();
        services.AddActivity<BindModelStateTask, BindModelStateTaskDisplayDriver>();
    }
}
