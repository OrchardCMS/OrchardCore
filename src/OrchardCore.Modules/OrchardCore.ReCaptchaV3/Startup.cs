using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.ReCaptchaV3.Configuration;
using OrchardCore.ReCaptchaV3.Core;
using OrchardCore.ReCaptchaV3.Drivers;
using OrchardCore.Settings;
using OrchardCore.Settings.Deployment;

namespace OrchardCore.ReCaptchaV3
{
    [Feature("OrchardCore.ReCaptchaV3")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddReCaptchaV3();

            services.AddScoped<IDisplayDriver<ISite>, ReCaptchaV3SettingsDisplayDriver>();
            services.AddScoped<INavigationProvider, AdminMenu>();
        }
    }

    [Feature("OrchardCore.ReCaptchaV3")]
    [RequireFeatures("OrchardCore.Deployment")]
    public class DeploymentStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSiteSettingsPropertyDeploymentStep<ReCaptchaV3Settings, DeploymentStartup>(S => S["ReCaptchaV3 settings"], S => S["Exports the ReCaptchaV3 settings."]);
        }
    }
}
