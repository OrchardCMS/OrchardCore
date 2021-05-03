using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.AuditTrail.Providers;
using OrchardCore.AuditTrail.Services;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Contents.AuditTrail.Controllers;
using OrchardCore.Contents.AuditTrail.Handlers;
using OrchardCore.Contents.AuditTrail.Providers;
using OrchardCore.Contents.AuditTrail.Services;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;

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
            services.AddScoped<IAuditTrailEventProvider, ContentAuditTrailEventProvider>();
            services.AddScoped<IAuditTrailEventHandler, ContentAuditTrailEventHandler>();
            services.AddScoped<IAuditTrailDisplayHandler, ContentAuditTrailDisplayHandler>();

            services.AddScoped<ContentHandler>();
            services.AddScoped<IContentHandler>(sp => sp.GetRequiredService<ContentHandler>());
            services.AddScoped<IAuditTrailContentHandler>(sp => sp.GetRequiredService<ContentHandler>());
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var contentControllerName = typeof(ContentController).ControllerName();

            routes.MapAreaControllerRoute(
               name: "DetailContentItem",
               areaName: "OrchardCore.Contents",
               pattern: _adminOptions.AdminUrlPrefix + "/AuditTrail/Content/{versionNumber}/{auditTrailEventId}",
               defaults: new { controller = contentControllerName, action = nameof(ContentController.Detail) }
           );

            routes.MapAreaControllerRoute(
               name: "RestoreContentItem",
               areaName: "OrchardCore.Contents",
               pattern: _adminOptions.AdminUrlPrefix + "/AuditTrail/Content/{auditTrailEventId}",
               defaults: new { controller = contentControllerName, action = nameof(ContentController.Restore) }
           );
        }
    }
}
