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
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IDisplayDriver<ISite>, ReCaptchaSettingsDisplayDriver>();
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IRegistrationEvents, RegistrationEventHandlers>();
            services.AddScoped<IAccountEvents, AccountEventHandlers>();
            services.AddScoped<IForgotPasswordEvents, ForgotPasswordEventHandlers>();
            services.Configure<MvcOptions>((options) =>
            {
                options.Filters.Add(typeof(ReCaptchaFilter));
            });
            services.AddReCaptcha();
        }
    }
}
