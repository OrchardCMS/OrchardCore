using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Modules.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Manifests;
using Orchard.Environment.Shell.Data;

namespace Orchard.Cms.Web
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddJsonFile("logging.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"logging.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddThemingHost();
            services.AddManifestDefinition("Theme.txt", "theme");
            services.AddExtensionLocation("Themes");
            services.AddSitesFolder("App_Data", "Sites");
            services.AddModuleServices(Configuration).WithDefaultFeatures("Orchard.Commons");
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration);

            app.UseModules();
        }
    }
}