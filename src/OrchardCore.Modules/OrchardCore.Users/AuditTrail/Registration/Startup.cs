using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.Modules;
using OrchardCore.Users.Events;

namespace OrchardCore.Users.AuditTrail.Registration;

[RequireFeatures("OrchardCore.Users.AuditTrail", UserConstants.Features.UserRegistration)]
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IRegistrationFormEvents, UserRegistrationEventHandler>();
        services.AddTransient<IConfigureOptions<AuditTrailOptions>, UserRegistrationAuditTrailEventConfiguration>();
    }
}
