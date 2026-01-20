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
        public override void ConfigureServices(IServiceCollection services)
        {
            services.Configure<TemplateOptions>(o =>
            {
                o.MemberAccessStrategy.Register<ReCaptchaPart>();
            });

            services.AddContentPart<ReCaptchaPart>()
                .UseDisplayDriver<ReCaptchaPartDisplayDriver>();

            services.AddDataMigration<Migrations>();
        }
    }
}
