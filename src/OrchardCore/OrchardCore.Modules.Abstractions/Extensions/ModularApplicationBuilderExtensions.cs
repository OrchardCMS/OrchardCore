using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using OrchardCore.Modules;

namespace Microsoft.AspNetCore.Builder
{
    public static class ModularApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseStaticFilesModules(this IApplicationBuilder app)
        {
            var env = app.ApplicationServices.GetRequiredService<IHostingEnvironment>();

            IFileProvider fileProvider;
            if (env.IsDevelopment())
            {
                var fileProviders = new List<IFileProvider>();
                fileProviders.Add(new ModuleProjectStaticFileProvider(env));
                fileProviders.Add(new ModuleEmbeddedStaticFileProvider(env));
                fileProvider = new CompositeFileProvider(fileProviders);
            }
            else
            {
                fileProvider = new ModuleEmbeddedStaticFileProvider(env);
            }

            app.UseStaticFiles(new StaticFileOptions
            {
                RequestPath = "",
                FileProvider = fileProvider
            });

            return app;
        }
    }
}