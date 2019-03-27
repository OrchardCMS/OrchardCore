using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
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

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            if (!_hostingEnvironment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }

            // c.f. https://docs.asp.net/en/latest/fundamentals/error-handling.html#id3
            app.UseStatusCodePagesWithReExecute("/Error", "?status={0}");

            app.Use((context, next) =>
            {
                var builder = next();

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

                return builder;
            });

            routes.MapAreaRoute(
                name: "Diagnostics.Error",
                areaName: "OrchardCore.Diagnostics",
                template: "Error",
                defaults: new { controller = "Diagnostics", action = "Error" }
            );
        }

        public override void ConfigureServices(IServiceCollection services)
        {
        }
    }
}
