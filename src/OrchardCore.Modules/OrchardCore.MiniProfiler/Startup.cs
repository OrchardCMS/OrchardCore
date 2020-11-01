using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.Modules;
using YesSql;

namespace OrchardCore.MiniProfiler
{
    public class Startup : StartupBase
    {
        // Early in the pipeline to wrap all other middleware
        public override int Order => -500;

        public override void ConfigureServices(IServiceCollection services)
        {
            services.Configure<MvcOptions>((options) =>
            {
                options.Filters.Add(typeof(MiniProfilerFilter));
            });

            services.AddScoped<IShapeDisplayEvents, ShapeStep>();

            services.AddMiniProfiler();
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            app.UseMiniProfiler();

            var store = serviceProvider.GetService<IStore>();

            var factory = store.Configuration.ConnectionFactory;
            var profilerFactory = new MiniProfilerConnectionFactory(factory);
            store.Configuration.ConnectionFactory = profilerFactory;
            var dialect = SqlDialectFactory.For(factory.DbConnectionType);
            SqlDialectFactory.Register(profilerFactory.DbConnectionType, dialect);

            using (var connection = factory.CreateConnection())
            {
                var interpreter = CommandInterpreterFactory.For(connection);
                CommandInterpreterFactory.Register(profilerFactory.DbConnectionType, d => interpreter);
            }
        }
    }
}
