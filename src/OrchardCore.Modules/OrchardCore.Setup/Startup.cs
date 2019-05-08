using System;
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
            app.UsePortableObjectLocalization(localizationOptions, _defaultCulture, _supportedCultures);

            routes.MapAreaRoute(
                name: "Setup",
                areaName: "OrchardCore.Setup",
                template: "",
                defaults: new { controller = "Setup", action = "Index" }
            );
        }
    }
}
