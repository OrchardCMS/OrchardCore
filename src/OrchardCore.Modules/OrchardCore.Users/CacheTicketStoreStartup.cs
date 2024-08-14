using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;

namespace OrchardCore.Users.Authentication;

[Feature("OrchardCore.Users.Authentication.CacheTicketStore")]
public sealed class CacheTicketStoreStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.ConfigureOptions<CookieAuthenticationOptionsConfigure>();
    }
}
