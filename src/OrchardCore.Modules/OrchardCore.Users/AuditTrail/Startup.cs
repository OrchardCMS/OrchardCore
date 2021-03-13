using Microsoft.Extensions.DependencyInjection;
using OrchardCore.AuditTrail.Providers;
using OrchardCore.Modules;
using OrchardCore.Users.AuditTrail.Handlers;
using OrchardCore.Users.AuditTrail.Providers;
using OrchardCore.Users.Events;
using OrchardCore.Users.Handlers;

namespace OrchardCore.Users.AuditTrail
{
    [RequireFeatures("OrchardCore.AuditTrail")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IAuditTrailEventProvider, UserAuditTrailEventProvider>();

            services.AddScoped<IUserEventHandler, UserEventHandler>();
            services.AddScoped<ILoginFormEvent, UserEventHandler>();
            services.AddScoped<IPasswordRecoveryFormEvents, UserEventHandler>();
            services.AddScoped<IRegistrationFormEvents, UserEventHandler>();
        }
    }
}
