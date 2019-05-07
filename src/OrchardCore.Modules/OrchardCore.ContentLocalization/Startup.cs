using Fluid;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentLocalization.Drivers;
using OrchardCore.ContentLocalization.Indexing;
using OrchardCore.ContentLocalization.Security;
using OrchardCore.ContentLocalization.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.Indexing;
using OrchardCore.Modules;
using OrchardCore.Security.Permissions;

namespace OrchardCore.ContentLocalization
{
    public class Startup : StartupBase
    {
        static Startup()
        {
            TemplateContext.GlobalMemberAccessStrategy.Register<LocalizationPartViewModel>();
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IContentPartDisplayDriver, LocalizationPartDisplayDriver>();
            services.AddScoped<IContentPartIndexHandler, LocalizationPartIndexHandler>();
            services.AddContentLocalization();

            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<IAuthorizationHandler, LocalizedContentAuthorizationHandler>();

        }
    }
}