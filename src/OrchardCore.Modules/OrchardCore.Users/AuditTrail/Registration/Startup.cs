using Microsoft.Extensions.DependencyInjection;
using OrchardCore.AuditTrail.Providers;
using OrchardCore.Modules;
using OrchardCore.Users.Events;

namespace OrchardCore.Users.AuditTrail.Registration
{
    [RequireFeatures("OrchardCore.Users.AuditTrail", "OrchardCore.Users.Registration")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IRegistrationFormEvents, UserRegistrationEventHandler>();
            services.AddScoped<IAuditTrailEventProvider, UserRegistrationAuditTrailEventProvider>();
        }
    }
}
