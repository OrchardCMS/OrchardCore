using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;

namespace Orchard.Diagnostics
{
    public class Startup : StartupBase
    {
        private readonly FileExtensionContentTypeProvider _contentTypeProvider = new FileExtensionContentTypeProvider();

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var env = serviceProvider.GetService<IHostingEnvironment>();

            if (!env.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }

            // c.f. https://docs.asp.net/en/latest/fundamentals/error-handling.html#id3
            app.UseStatusCodePagesWithReExecute("/Error", "?status={0}");

            app.Use((context, next) =>
            {
                var builder = next();

                if (context.Response.StatusCode != 200)
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
                areaName: "Orchard.Diagnostics",
                template: "Error",
                defaults: new { controller = "Diagnostics", action = "Error" }
            );
        }

        public override void ConfigureServices(IServiceCollection services)
        {
        }
    }
}
