using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.AuditTrail.Models;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Users.AuditTrail.Drivers;
using OrchardCore.Users.AuditTrail.Handlers;
using OrchardCore.Users.AuditTrail.Services;
using OrchardCore.Users.Events;
using OrchardCore.Users.Handlers;

namespace OrchardCore.Users.AuditTrail
{
    [Feature("OrchardCore.Users.AuditTrail")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IConfigureOptions<AuditTrailOptions>, UserAuditTrailEventConfiguration>();

            services.AddScoped<UserEventHandler, UserEventHandler>()
                .AddScoped<IUserEventHandler>(sp => sp.GetRequiredService<UserEventHandler>())
                .AddScoped<ILoginFormEvent>(sp => sp.GetRequiredService<UserEventHandler>());

            services.AddScoped<IDisplayDriver<AuditTrailEvent>, AuditTrailUserEventDisplayDriver>();
        }
    }
}
