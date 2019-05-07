using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Modules;

namespace OrchardCore.Setup
{
    public class Startup : StartupBase
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json");

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddPortableObjectLocalization(options => options.ResourcesPath = "Localization");
            services.AddSetup();
        }

		public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var options = serviceProvider.GetService<IOptions<RequestLocalizationOptions>>().Value;
            var defaultCulture = Configuration["OrchardCore:OrchardCore.Setup:DefaultCulture"];
            var supportedCultures = Configuration.GetSection("OrchardCore:OrchardCore.Setup:SupportedCultures").Get<string[]>();

            if (!string.IsNullOrEmpty(defaultCulture))
            {
                options.SetDefaultCulture(defaultCulture);
            }

            if (supportedCultures.Length > 0)
            {
                var supportedCulture = new[] { options.DefaultRequestCulture.Culture.Name }
                    .Concat(supportedCultures)
                    .Distinct()
                    .ToArray();

                options
                    .AddSupportedCultures(supportedCultures)
                    .AddSupportedUICultures(supportedCultures)
                    ;
            }

            app.UseRequestLocalization(options);

            routes.MapAreaRoute(
                name: "Setup",
                areaName: "OrchardCore.Setup",
                template: "",
                defaults: new { controller = "Setup", action = "Index" }
            );
        }
    }
}
