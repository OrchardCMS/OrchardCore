using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Hosting;
using OrchardCore.Modules;

namespace OrchardCore.Diagnostics
{
    public class Startup : StartupBase
    {
        private readonly FileExtensionContentTypeProvider _contentTypeProvider = new FileExtensionContentTypeProvider();

        private readonly IHostEnvironment _hostingEnvironment;

        public Startup(IHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public override void ConfigureBeforeRouting(IApplicationBuilder app, IServiceProvider serviceProvider)
        {
            if (!_hostingEnvironment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStatusCodePagesWithReExecute("/Error/{0}");

            app.Use(async (context, next) =>
            {
                await next();

                if (context.Response.StatusCode < 200 || context.Response.StatusCode >= 400)
                {
                    string contentType;
                    if (_contentTypeProvider.TryGetContentType(context.Request.Path, out contentType))
                    {
                        var statusCodePagesFeature = context.Features.Get<IStatusCodePagesFeature>();
                        if (statusCodePagesFeature != null)
                        {
                            statusCodePagesFeature.Enabled = false;
                        }
                    }
                }
            });
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaControllerRoute(
                name: "Diagnostics.Error",
                areaName: "OrchardCore.Diagnostics",
                pattern: "Error/{status?}",
                defaults: new { controller = "Diagnostics", action = "Error" }
            );
        }
    }
}
