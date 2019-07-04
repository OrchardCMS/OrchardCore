using System;
using System.Linq;
using Fluid;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentLocalization.Drivers;
using OrchardCore.ContentLocalization.Handlers;
using OrchardCore.ContentLocalization.Indexing;
using OrchardCore.ContentLocalization.Models;
using OrchardCore.ContentLocalization.Records;
using OrchardCore.ContentLocalization.Security;
using OrchardCore.ContentLocalization.Services;
using OrchardCore.ContentLocalization.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Indexing;
using OrchardCore.Modules;
using OrchardCore.Security.Permissions;
using YesSql;

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
            services.AddScoped<IAuthorizationHandler, LocalizeContentAuthorizationHandler>();
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
                options.RequestCultureProviders.Insert(0, new ContentRequestCultureProvider());
            });

            services.AddScoped<IContentPartHandler, LocalizationPartHandler>();
            services.AddSingleton<ILocalizationEntries, LocalizationEntries>();
        }

        public override void Configure(IApplicationBuilder builder, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaRoute(
               name: "RedirectToLocalizedContent",
               areaName: "OrchardCore.ContentLocalization",
               template: "RedirectToLocalizedContent",
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
}
