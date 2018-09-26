using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.ReCaptcha.Core;
using OrchardCore.ReCaptcha.Drivers;
using OrchardCore.ReCaptcha.Shapes;
using OrchardCore.ReCaptcha.Users.Handlers;
using OrchardCore.ReCaptcha.Users.Shapes;
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
            services.AddScoped<IShapeTableProvider, ReCaptchaShapes>();

            services.AddReCaptcha();
        }
    }

    [Feature("OrchardCore.ReCaptcha.User.RegisterAccount")]
    public class RegisterAccountStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IRegistrationEvents, RegistrationEventHandlers>();
            services.AddScoped<IShapeFactoryEvents, AfterRegistrationShapes>();
        }
    }

    [Feature("OrchardCore.ReCaptcha.User.Login")]
    public class AccountStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IAccountEvents, AccountEventHandlers>();
            services.AddScoped<IShapeFactoryEvents, AfterLoginShapes>();
        }
    }

    [Feature("OrchardCore.ReCaptcha.User.ForgotPassword")]
    public class ForgotPassword : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IForgotPasswordEvents, ForgotPasswordEventHandlers>();
            services.AddScoped<IShapeFactoryEvents, AfterForgotPasswordShapes>();
        }
    }
}
