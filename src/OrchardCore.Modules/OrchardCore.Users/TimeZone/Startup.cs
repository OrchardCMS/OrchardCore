using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Users.Models;
using OrchardCore.Users.TimeZone.Drivers;
using OrchardCore.Users.TimeZone.Services;

namespace OrchardCore.Users.TimeZone
{
    [Feature("OrchardCore.Users.TimeZone")]
    public class Startup : StartupBase
    {
        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<ITimeZoneSelector, UserTimeZoneSelector>();
            services.AddScoped<UserTimeZoneService>();

            services.AddScoped<IDisplayDriver<User>, UserTimeZoneDisplayDriver>();
        }
    }
}
