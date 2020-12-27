using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using OrchardCore.Modules;

namespace Microsoft.AspNetCore.Builder
{
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Enables multi-tenant requests support for the current path.
        /// </summary>
        public static IApplicationBuilder UseOrchardCore(this IApplicationBuilder app, Action<IApplicationBuilder> configure = null)
        {
            var env = app.ApplicationServices.GetRequiredService<IHostEnvironment>();
            var appContext = app.ApplicationServices.GetRequiredService<IApplicationContext>();

            env.ContentRootFileProvider = new CompositeFileProvider(
                new ModuleEmbeddedFileProvider(appContext),
                env.ContentRootFileProvider);

            // Init also the web host 'ContentRootFileProvider'.
            app.ApplicationServices.GetRequiredService<IWebHostEnvironment>()
                .ContentRootFileProvider = env.ContentRootFileProvider;

            app.UseMiddleware<PoweredByMiddleware>();

            // Ensure the shell tenants are loaded when a request comes in
            // and replaces the current service container for the tenant's one.
            app.UseMiddleware<TenantContainerMiddleware>();

            // Serve static files from modules.
            app.UseModuleStaticFiles();

            // Create a new scope on the tenant container.
            app.UseMiddleware<TenantScopeMiddleware>();

            configure?.Invoke(app);

            // Forward the request to the tenant specific pipeline.
            app.UseMiddleware<TenantRouterMiddleware>(app.ServerFeatures);

            return app;
        }

        private static IApplicationBuilder UseModuleStaticFiles(this IApplicationBuilder app)
        {
            var options = app.ApplicationServices.GetRequiredService<IOptions<StaticFileOptions>>().Value;
            var fileProvider = app.ApplicationServices.GetRequiredService<IModuleStaticFileProvider>();

            var configuration = app.ApplicationServices.GetRequiredService<IConfiguration>();
            var cacheControl = configuration.GetValue("OrchardCore:StaticFileOptions:CacheControl", "public, max-age=2592000, s-max-age=31557600");

            options.RequestPath = "";
            options.FileProvider = fileProvider;

            // Cache static files for a year as they are coming from embedded resources and should not vary
            options.OnPrepareResponse = ctx =>
            {
                ctx.Context.Response.Headers[HeaderNames.CacheControl] = cacheControl;
            };

            return app.UseStaticFiles(options);
        }
    }
}
