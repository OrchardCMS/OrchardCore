using Fluid;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.Data.Migration;
using OrchardCore.Modules;

namespace OrchardCore.ReCaptcha.Forms
{
    [RequireFeatures("OrchardCore.Forms", "OrchardCore.ReCaptcha")]
    public class Startup : StartupBase
    {
        static Startup()
        {
            TemplateContext.GlobalMemberAccessStrategy.Register<ReCaptchaPart>();
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddContentPart<ReCaptchaPart>()
                .UseDisplayDriver<ReCaptchaPartDisplay>();

            services.AddScoped<IDataMigration, Migrations>();
        }
    }
}
