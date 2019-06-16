using System.Linq;
using Fluid;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
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

    [Feature("OrchardCore.ContentLocalization.ContentCulturePicker")]
    public class ContentPickerStartup : StartupBase
    {

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IContentCulturePickerService, ContentCulturePickerService>();

            services.Configure<RequestLocalizationOptions>(options =>
            {
                var index = options.RequestCultureProviders.ToList()
                    .FindIndex(p => p is AcceptLanguageHeaderRequestCultureProvider);

                if (index == -1)
                {
                    index = options.RequestCultureProviders.Count();
                }

                options.RequestCultureProviders.Insert(index, new ContentRequestCultureProvider());
            });
        }
    }
}
