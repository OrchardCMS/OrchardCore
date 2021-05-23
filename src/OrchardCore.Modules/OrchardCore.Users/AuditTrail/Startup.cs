using Microsoft.Extensions.DependencyInjection;
using OrchardCore.AuditTrail.Models;
using OrchardCore.AuditTrail.Providers;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Users.AuditTrail.Drivers;
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

            // TODO register and resolve same instance.

            services.AddScoped<IUserEventHandler, UserEventHandler>();
            services.AddScoped<ILoginFormEvent, UserEventHandler>();
            services.AddScoped<IPasswordRecoveryFormEvents, UserEventHandler>();
            services.AddScoped<IRegistrationFormEvents, UserEventHandler>();
            services.AddScoped<IDisplayDriver<AuditTrailEvent>, AuditTrailUserEventDisplayDriver>();
        }
    }
}
