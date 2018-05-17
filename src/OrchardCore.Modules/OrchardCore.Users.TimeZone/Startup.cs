using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Users.TimeZone.Drivers;
using OrchardCore.Users.Models;
using OrchardCore.DisplayManagement.TimeZone;
using OrchardCore.Users.TimeZone.Services;

namespace OrchardCore.UserProfile
{
    public class Startup : StartupBase
    {

        public override void Configure(IApplicationBuilder builder, IRouteBuilder routes, IServiceProvider serviceProvider)
        {

        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IDisplayDriver<User>, UserProfileDisplayDriver>();
            services.AddScoped<ITimeZoneSelector, UserTimeZoneSelector>();
            services.AddSingleton<IUserTimeZoneService, UserTimeZoneService>();
        }
    }
}