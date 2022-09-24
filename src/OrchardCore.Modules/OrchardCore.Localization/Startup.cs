using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Localization.Drivers;
using OrchardCore.Localization.Models;
using OrchardCore.Localization.Services;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using OrchardCore.Settings.Deployment;

namespace OrchardCore.Localization
{
    /// <summary>
    /// Represents a localization module entry point.
    /// </summary>
    public class Startup : StartupBase
    {
        private readonly IShellConfiguration _shellConfiguration;

        public Startup(IShellConfiguration shellConfiguration)
        {
            _shellConfiguration = shellConfiguration;
        }

        public override int ConfigureOrder => -100;

        /// <inheritdocs />
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IDisplayDriver<ISite>, LocalizationSettingsDisplayDriver>();
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<ILocalizationService, LocalizationService>();

            services.AddPortableObjectLocalization(options => options.ResourcesPath = "Localization").
                AddDataAnnotationsPortableObjectLocalization();

            services.Replace(ServiceDescriptor.Singleton<ILocalizationFileLocationProvider, ModularPoFileLocationProvider>());

            services.Configure<CultureLocalizationOptions>(_shellConfiguration.GetSection("OrchardCore_Localization"));
        }

        /// <inheritdocs />
        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var localizationService = serviceProvider.GetService<ILocalizationService>();

            var defaultCulture = localizationService.GetDefaultCultureAsync().GetAwaiter().GetResult();
            var supportedCultures = localizationService.GetSupportedCulturesAsync().GetAwaiter().GetResult();
            //var useUserSelectedCultureSettings = localizationService.GetCultureSettingsAsync()
            //    .GetAwaiter().GetResult() == CultureSettings.User;

            var requestLocalizationOptions = serviceProvider.GetService<IOptions<RequestLocalizationOptions>>().Value;
            var cultureLocalizationOptions = serviceProvider.GetService<IOptions<CultureLocalizationOptions>>().Value;
            var useUserSelectedCultureSettings = cultureLocalizationOptions.CultureSettings == CultureSettings.User;

            requestLocalizationOptions
                .SetDefaultCulture(defaultCulture)
                .AddSupportedCultures(supportedCultures, useUserSelectedCultureSettings)
                .AddSupportedUICultures(supportedCultures, useUserSelectedCultureSettings);

            app.UseRequestLocalization(requestLocalizationOptions);
        }
    }

    [RequireFeatures("OrchardCore.Deployment")]
    public class LocalizationDeploymentStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSiteSettingsPropertyDeploymentStep<LocalizationSettings, LocalizationDeploymentStartup>(S => S["Culture settings"], S => S["Exports the culture settings."]);
        }
    }

    [Feature("OrchardCore.Localization.ContentLanguageHeader")]
    public class ContentLanguageHeaderStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.Configure<RequestLocalizationOptions>(options => options.ApplyCurrentCultureToResponseHeaders = true);
        }
    }
}
