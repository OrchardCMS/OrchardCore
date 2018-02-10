using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Data.Migration;
using OrchardCore.Demo.Users.EntityFrameworkCore.Data;
using OrchardCore.Modules;
using OrchardCore.Users.EntityFrameworkCore;

namespace OrchardCore.Demo.Users.EntityFrameworkCore
{
    [Feature("OrchardCore.Demo.Users.EntityFrameworkCore")]
    public class Startup : StartupBase
    {

        public override void Configure(IApplicationBuilder builder, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IDataMigration, Migrations>();
            services.AddDbContext<UserIdentityDbContext>();
            services.AddEntityFrameworkUserDataStore<UserIdentityDbContext,int>();
        }
    }
}
