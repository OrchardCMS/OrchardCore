using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;

namespace OrchardCore.Setup
{
    public class Startup : StartupBase
    {
        private readonly string _defaultCulture;
        private readonly string[] _supportedCultures;

        public Startup(IShellConfiguration shellConfiguration)
        {
            var configurationSection = shellConfiguration.GetSection("OrchardCore.Setup");

            _defaultCulture = configurationSection["DefaultCulture"];
            _supportedCultures = configurationSection.GetSection("SupportedCultures").Get<string[]>();
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddPortableObjectLocalization(options => options.ResourcesPath = "Localization");
            services.AddSetup();
        }

		public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var options = serviceProvider.GetService<IOptions<RequestLocalizationOptions>>().Value;

            if (!String.IsNullOrEmpty(_defaultCulture))
            {
                options.SetDefaultCulture(_defaultCulture);
            }

            if (_supportedCultures.Length > 0)
            {
                var supportedCulture = new[] { options.DefaultRequestCulture.Culture.Name }
                    .Concat(_supportedCultures)
                    .Distinct()
                    .ToArray();

                options
                    .AddSupportedCultures(supportedCulture)
                    .AddSupportedUICultures(supportedCulture)
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
