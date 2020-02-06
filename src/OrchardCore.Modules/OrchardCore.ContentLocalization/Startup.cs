using System;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Fluid;
using OrchardCore.Admin;
using OrchardCore.ContentLocalization.Controllers;
using OrchardCore.ContentLocalization.Drivers;
using OrchardCore.ContentLocalization.Handlers;
using OrchardCore.ContentLocalization.Indexing;
using OrchardCore.ContentLocalization.Liquid;
using OrchardCore.ContentLocalization.Models;
using OrchardCore.ContentLocalization.Records;
using OrchardCore.ContentLocalization.Security;
using OrchardCore.ContentLocalization.Services;
using OrchardCore.ContentLocalization.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Indexing;
using OrchardCore.Liquid;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using YesSql;

namespace OrchardCore.ContentLocalization
{
    public class Startup : StartupBase
    {
        private readonly AdminOptions _adminOptions;

        static Startup()
        {
            TemplateContext.GlobalMemberAccessStrategy.Register<LocalizationPartViewModel>();
        }

        public Startup(IOptions<AdminOptions> adminOptions)
        {
            _adminOptions = adminOptions.Value;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IContentPartDisplayDriver, LocalizationPartDisplayDriver>();
            services.AddScoped<IContentPartIndexHandler, LocalizationPartIndexHandler>();
            services.AddSingleton<ILocalizationEntries, LocalizationEntries>();
            services.AddScoped<IContentPartHandler, LocalizationPartHandler>();
            services.AddContentLocalization();

            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<IAuthorizationHandler, LocalizeContentAuthorizationHandler>();
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaControllerRoute(
                name: "ContentLocalization.Localize",
                areaName: "OrchardCore.ContentLocalization",
                pattern: _adminOptions.AdminUrlPrefix + "/ContentLocalization",
                defaults: new { controller = typeof(AdminController).ControllerName(), action = nameof(AdminController.Localize) }
            );
        }
    }

    [Feature("OrchardCore.ContentLocalization.ContentCulturePicker")]
    public class ContentPickerStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IContentCulturePickerService, ContentCulturePickerService>();
            services.AddScoped<IDisplayDriver<ISite>, ContentCulturePickerSettingsDriver>();
            services.Configure<RequestLocalizationOptions>(options =>
            {
                options.AddInitialRequestCultureProvider(new ContentRequestCultureProvider());
            });
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaControllerRoute(
               name: "RedirectToLocalizedContent",
               areaName: "OrchardCore.ContentLocalization",
               pattern: "RedirectToLocalizedContent",
               defaults: new { controller = "ContentCulturePicker", action = "RedirectToLocalizedContent" }
           );

            var session = serviceProvider.GetRequiredService<ISession>();
            var entries = serviceProvider.GetRequiredService<ILocalizationEntries>();

            var indexes = session.QueryIndex<LocalizedContentItemIndex>(i => i.Published)
                .ListAsync().GetAwaiter().GetResult();

            entries.AddEntries(indexes.Select(i => new LocalizationEntry
            {
                ContentItemId = i.ContentItemId,
                LocalizationSet = i.LocalizationSet,
                Culture = i.Culture.ToLowerInvariant()
            }));
        }
    }
    [RequireFeatures("OrchardCore.Liquid")]
    public class LiquidStartup : StartupBase
    {
        static LiquidStartup()
        {
            TemplateContext.GlobalMemberAccessStrategy.Register<CultureInfo>();
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddLiquidFilter<ContentLocalizationFilter>("localization_set");
            services.AddLiquidFilter<SwitchCultureUrlFilter>("switch_culture_url");
        }
    }
}
