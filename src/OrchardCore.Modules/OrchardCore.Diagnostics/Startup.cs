using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrchardCore.Modules;
using OrchardCore.Mvc.Routing;
using OrchardCore.Routing;

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

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            if (!_hostingEnvironment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }

            // c.f. https://docs.asp.net/en/latest/fundamentals/error-handling.html#id3
            app.UseStatusCodePagesWithReExecute("/Error", "?status={0}");

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
                    else
                    {
                        // Workaround of c.f. https://github.com/aspnet/AspNetCore/issues/11555
                        var endpointDataSource = context.RequestServices.GetRequiredService<EndpointDataSource>();
                        var routeValues = new RouteValueDictionary(new
                        {
                            area = "OrchardCore.Diagnostics",
                            controller = "Diagnostics",
                            action = "Error"
                        });

                        var endpoint = endpointDataSource.Endpoints
                            .Where(e => e.Match(routeValues))
                            .FirstOrDefault();

                        if (endpoint != null)
                        {
                            endpoint.Select(context, routeValues);
                        }
                    }
                }
            });

            routes.MapAreaControllerRoute(
                name: "Diagnostics.Error",
                areaName: "OrchardCore.Diagnostics",
                pattern: "Error",
                defaults: new { controller = "Diagnostics", action = "Error" }
            );
        }

        public override void ConfigureServices(IServiceCollection services)
        {
        }
    }
}
