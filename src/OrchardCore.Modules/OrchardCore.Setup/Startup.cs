using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Localization;
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
            services.Replace(ServiceDescriptor.Singleton<ILocalizationFileLocationProvider, ModularPoFileLocationProvider>());

            services.AddSetup();
        }

		public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var localizationOptions = serviceProvider.GetService<IOptions<RequestLocalizationOptions>>().Value;

            if (!String.IsNullOrEmpty(_defaultCulture))
            {
                localizationOptions.SetDefaultCulture(_defaultCulture);
            }

            if (_supportedCultures?.Length > 0)
            {
                var supportedCultures =_supportedCultures
                    .Concat(new[] { localizationOptions.DefaultRequestCulture.Culture.Name })
                    .Distinct()
                    .ToArray();

                localizationOptions
                    .AddSupportedCultures(supportedCultures)
                    .AddSupportedUICultures(supportedCultures);
            }

            app.UseRequestLocalization(localizationOptions);

            routes.MapAreaRoute(
                name: "Setup",
                areaName: "OrchardCore.Setup",
                template: "",
                defaults: new { controller = "Setup", action = "Index" }
            );
        }
    }
}
