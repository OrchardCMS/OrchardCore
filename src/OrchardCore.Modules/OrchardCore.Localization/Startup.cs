using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Shell;
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
        }

        /// <inheritdocs />
        public override async ValueTask ConfigureAsync(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var localizationService = serviceProvider.GetService<ILocalizationService>();

            var defaultCulture = await localizationService.GetDefaultCultureAsync();
            var supportedCultures = await localizationService.GetSupportedCulturesAsync();

            var cultureOptions = serviceProvider.GetService<IOptions<CultureOptions>>().Value;

            app.UseRequestLocalization(options =>
            {
                options.CultureInfoUseUserOverride = !cultureOptions.IgnoreSystemSettings;
                options.SetDefaultCulture(defaultCulture);
                options.AddSupportedCultures(supportedCultures);
                options.AddSupportedUICultures(supportedCultures);
            });
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

    [Feature("OrchardCore.Localization.AdminCulturePicker")]
    public class CulturePickerStartup : StartupBase
    {
        private readonly ShellSettings _shellSettings;
        private readonly AdminOptions _adminOptions;

        public CulturePickerStartup(IOptions<AdminOptions> adminOptions, ShellSettings shellSettings)
        {
            _shellSettings = shellSettings;
            _adminOptions = adminOptions.Value;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IDisplayDriver<Navbar>, AdminCulturePickerNavbarDisplayDriver>();

            services.Configure<RequestLocalizationOptions>(options =>
                options.AddInitialRequestCultureProvider(new AdminCookieCultureProvider(_shellSettings, _adminOptions)));
        }
    }
}
