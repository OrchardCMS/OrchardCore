using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.AuditTrail.Models;
using OrchardCore.AuditTrail.Providers;
using OrchardCore.AuditTrail.Services;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Contents.AuditTrail.Controllers;
using OrchardCore.Contents.AuditTrail.Drivers;
using OrchardCore.Contents.AuditTrail.Handlers;
using OrchardCore.Contents.AuditTrail.Models;
using OrchardCore.Contents.AuditTrail.Providers;
using OrchardCore.Contents.AuditTrail.Services;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Settings;

namespace OrchardCore.Contents.AuditTrail
{
    [RequireFeatures("OrchardCore.AuditTrail")]
    public class Startup : StartupBase
    {
        private readonly AdminOptions _adminOptions;

        public Startup(IOptions<AdminOptions> adminOptions)
        {
            _adminOptions = adminOptions.Value;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IDataMigration, Migrations>();
            services.AddContentPart<AuditTrailPart>()
                .UseDisplayDriver<AuditTrailPartDisplayDriver>();
                
            services.AddScoped<IContentTypePartDefinitionDisplayDriver, AuditTrailPartSettingsDisplayDriver>();
            services.AddScoped<IContentDisplayDriver, AuditTrailContentsDriver>();

            services.AddScoped<IAuditTrailEventProvider, ContentAuditTrailEventProvider>();
            services.AddScoped<IAuditTrailEventHandler, ContentAuditTrailEventHandler>();
            services.AddScoped<IDisplayDriver<ISite>, ContentAuditTrailSettingsDisplayDriver>();

            services.AddScoped<IDisplayDriver<AuditTrailEvent>, AuditTrailContentEventDisplayDriver>();

            services.AddScoped<IContentHandler, AuditTrailContentHandler>();
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var contentControllerName = typeof(AuditTrailContentController).ControllerName();

            routes.MapAreaControllerRoute(
               name: "DisplayContent",
               areaName: "OrchardCore.Contents",
               pattern: _adminOptions.AdminUrlPrefix + "/AuditTrail/Content/{versionNumber}/{auditTrailEventId}",
               defaults: new { controller = contentControllerName, action = nameof(AuditTrailContentController.Display) }
           );

            routes.MapAreaControllerRoute(
               name: "RestoreContent",
               areaName: "OrchardCore.Contents",
               pattern: _adminOptions.AdminUrlPrefix + "/AuditTrail/Content/{auditTrailEventId}",
               defaults: new { controller = contentControllerName, action = nameof(AuditTrailContentController.Restore) }
           );
        }
    }
}
