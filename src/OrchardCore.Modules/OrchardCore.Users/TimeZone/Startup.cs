using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Users.Handlers;
using OrchardCore.Users.Models;
using OrchardCore.Users.TimeZone.Drivers;
using OrchardCore.Users.TimeZone.Handlers;
using OrchardCore.Users.TimeZone.Services;

namespace OrchardCore.Users.TimeZone;

[Feature("OrchardCore.Users.TimeZone")]
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IUserTimeZoneService, UserTimeZoneService>();
        services.AddScoped<IUserEventHandler, UserEventHandler>();
        services.AddScoped<ITimeZoneSelector, UserTimeZoneSelector>();
        services.AddDisplayDriver<User, UserTimeZoneDisplayDriver>();
    }
}
