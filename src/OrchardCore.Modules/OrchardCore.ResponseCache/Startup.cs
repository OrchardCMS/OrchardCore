using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using OrchardCore.Modules;
using Microsoft.AspNetCore.ResponseCaching.Internal;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

namespace OrchardCore.ResponseCache
{
    public class Startup : StartupBase
    {
        public override int Order => -10;

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            app.UseResponseCaching();

            app.Use((context, next) =>
            {
                context.Response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue()
                {
                    Public = true,
                    MaxAge = TimeSpan.FromSeconds(60)
                };

                context.Response.Headers[HeaderNames.Vary] = new string[] { "Accept-Encoding" };

                return next();
            });
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IResponseCachingPolicyProvider, CustomResponseCachingPolicyProvider>();
            services.AddResponseCaching();
        }
    }
}
