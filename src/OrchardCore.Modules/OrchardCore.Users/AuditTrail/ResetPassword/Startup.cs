using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.Modules;
using OrchardCore.Users.Events;

namespace OrchardCore.Users.AuditTrail.ResetPassword;

[RequireFeatures("OrchardCore.Users.AuditTrail", UserConstants.Features.ResetPassword)]
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IPasswordRecoveryFormEvents, UserResetPasswordEventHandler>();
        services.AddTransient<IConfigureOptions<AuditTrailOptions>, UserResetPasswordAuditTrailEventConfiguration>();
    }
}
