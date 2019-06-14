using Fluid;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentLocalization.Drivers;
using OrchardCore.ContentLocalization.Indexing;
using OrchardCore.ContentLocalization.Security;
using OrchardCore.ContentLocalization.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement.Descriptors;
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

    [Feature("OrchardCore.ContentLocalization.ContentCulturePicker")]
    public class ContentPickerStartup : StartupBase
    {

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IContentCulturePickerService, ContentCulturePickerService>();
            // todo: this does not work, find another way.
            services.Configure<RequestLocalizationOptions>(options =>
            {
                options.RequestCultureProviders.Add(new ContentRequestCultureProvider()); 
            });
        }
    }

}