using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement.Display.ContentDisplay;
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
            services.AddScoped<IContentPartDisplayDriver, FormWorkflowPartDisplay>();
            services.AddActivity<ValidateAntiforgeryTokenTask, ValidateAntiforgeryTokenTaskDisplay>();
            services.AddActivity<ValidateNoCaptchaTask, ValidateNoCaptchaTaskDisplay>();
            services.AddActivity<AddModelValidationErrorTask, AddModelValidationErrorTaskDisplay>();
            services.AddActivity<ValidateFormTask, ValidateFormTaskDisplay>();
            services.AddActivity<ValidateFormFieldTask, ValidateFormFieldTaskDisplay>();
            services.AddActivity<BindModelStateTask, BindModelStateTaskDisplay>();
            services.AddAntiforgery();
        }
    }
}
