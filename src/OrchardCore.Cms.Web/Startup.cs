using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;

namespace OrchardCore.Cms.Web
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddOrchardCms()
                .ConfigureTenantServices<IOptions<ShellOptions>, ShellSettings>(
                (collection, options, settings) =>
                {
                    var directory = Directory.CreateDirectory(Path.Combine(
                        options.Value.ShellsApplicationDataPath,
                        options.Value.ShellsContainerName,
                        settings.Name, "DataProtection-Keys"));

                    collection.Add(new ServiceCollection()
                        .AddDataProtection()
                        .PersistKeysToFileSystem(directory)
                        .SetApplicationName(settings.Name)
                        .Services);
                }); ;
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseModules();
        }
    }
}