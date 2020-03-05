using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.ReCaptcha.Core;
using OrchardCore.ReCaptcha.Drivers;
using OrchardCore.ReCaptcha.Users.Handlers;
using OrchardCore.Settings;
using OrchardCore.Users.Events;

namespace OrchardCore.ReCaptcha
{
    [Feature("OrchardCore.ReCaptcha")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddReCaptcha();

            services.AddScoped<IDisplayDriver<ISite>, ReCaptchaSettingsDisplayDriver>();
            services.AddScoped<INavigationProvider, AdminMenu>();
        }
    }

    [Feature("OrchardCore.ReCaptcha.Users")]
    public class StartupUsers : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IRegistrationFormEvents, RegistrationFormEventHandler>();
            services.AddScoped<ILoginFormEvent, LoginFormEventEventHandler>();
            services.AddScoped<IPasswordRecoveryFormEvents, PasswordRecoveryFormEventEventHandler>();
            services.Configure<MvcOptions>((options) =>
            {
                options.Filters.Add(typeof(ReCaptchaLoginFilter));
            });
        }
    }
}
