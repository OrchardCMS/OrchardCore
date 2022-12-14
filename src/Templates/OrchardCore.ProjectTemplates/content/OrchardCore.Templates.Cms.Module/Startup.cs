#if (AddPart)
using Fluid;
#endif
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
#if (AddPart)
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using OrchardCore.Templates.Cms.Module.Drivers;
using OrchardCore.Templates.Cms.Module.Handlers;
using OrchardCore.Templates.Cms.Module.Models;
using OrchardCore.Templates.Cms.Module.Settings;
using OrchardCore.Templates.Cms.Module.ViewModels;
#endif
using OrchardCore.Modules;

namespace OrchardCore.Templates.Cms.Module
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
#if (AddPart)
            services.Configure<TemplateOptions>(o =>
            {
                o.MemberAccessStrategy.Register<MyTestPartViewModel>();
            });

            services.AddContentPart<MyTestPart>()
                .UseDisplayDriver<MyTestPartDisplayDriver>()
                .AddHandler<MyTestPartHandler>();

            services.AddScoped<IContentTypePartDefinitionDisplayDriver, MyTestPartSettingsDisplayDriver>();
            services.AddDataMigration<Migrations>();
#endif
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaControllerRoute(
                name: "Home",
                areaName: "OrchardCore.Templates.Cms.Module",
                pattern: "Home/Index",
                defaults: new { controller = "Home", action = "Index" }
            );
        }
    }
}
