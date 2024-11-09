using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Users.Handlers;
using OrchardCore.Users.Models;
using OrchardCore.Users.TimeZone.Drivers;
using OrchardCore.Users.TimeZone.Services;

namespace OrchardCore.Users.TimeZone;

[Feature("OrchardCore.Users.TimeZone")]
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<ITimeZoneSelector, UserTimeZoneSelector>();
        services.AddScoped<UserTimeZoneService>();
        services.AddScoped<IUserTimeZoneService>(sp => sp.GetRequiredService<UserTimeZoneService>());
        services.AddScoped<IUserEventHandler>(sp => sp.GetRequiredService<UserTimeZoneService>());
        services.AddScoped<IDisplayDriver<User>, UserTimeZoneDisplayDriver>();
    }
}
