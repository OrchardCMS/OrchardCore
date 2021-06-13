using Microsoft.Extensions.DependencyInjection;
using OrchardCore.AuditTrail.Providers;
using OrchardCore.Modules;
using OrchardCore.Users.Events;

namespace OrchardCore.Users.AuditTrail.ResetPassword
{
    [RequireFeatures("OrchardCore.Users.AuditTrail", "OrchardCore.Users.ResetPassword")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IPasswordRecoveryFormEvents, UserResetPasswordEventHandler>();
            services.AddScoped<IAuditTrailEventProvider, UserResetPasswordAuditTrailEventProvider>();
        }
    }
}
