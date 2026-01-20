using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;

namespace OrchardCore.Contents.Deployment.Download
{
    [Feature("OrchardCore.Contents.Deployment.Download")]
    public class DownloadStartup : StartupBase
    {
        private readonly AdminOptions _adminOptions;

        public DownloadStartup(IOptions<AdminOptions> adminOptions)
        {
            _adminOptions = adminOptions.Value;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IContentDisplayDriver, DownloadContentDriver>();
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var downloadControllerName = typeof(DownloadController).ControllerName();

            routes.MapAreaControllerRoute(
               name: "DownloadDisplay",
               areaName: "OrchardCore.Contents",
               pattern: _adminOptions.AdminUrlPrefix + "/Download/Display/{contentItemId}",
               defaults: new { controller = downloadControllerName, action = nameof(DownloadController.Display) }
           );

            routes.MapAreaControllerRoute(
               name: "DownloadDownload",
               areaName: "OrchardCore.Contents",
               pattern: _adminOptions.AdminUrlPrefix + "/Download/Download/{contentItemId}",
               defaults: new { controller = downloadControllerName, action = nameof(DownloadController.Download) }
           );
        }
    }
}
