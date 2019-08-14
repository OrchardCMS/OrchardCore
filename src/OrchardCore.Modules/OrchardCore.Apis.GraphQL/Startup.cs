using System;
using GraphQL;
using GraphQL.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrchardCore.Apis.GraphQL.Services;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Apis.GraphQL
{
    public class Startup : StartupBase
    {
        private readonly IShellConfiguration _configuration;
        private readonly IHostingEnvironment _hostingEnvironment;

        public Startup(IShellConfiguration configuration,
            IHostingEnvironment hostingEnvironment)
        {
            _configuration = configuration;
            _hostingEnvironment = hostingEnvironment;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IDependencyResolver, RequestServicesDependencyResolver>();
            services.AddSingleton<IDocumentExecuter, SerialDocumentExecuter>();
            services.AddSingleton<IDocumentWriter, DocumentWriter>();
            services.AddSingleton<ISchemaFactory, SchemaService>();

            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddTransient<INavigationProvider, AdminMenu>();
        }

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var exposeExceptions = _configuration.GetValue(
                $"OrchardCore.Apis.GraphQL:{nameof(GraphQLSettings.ExposeExceptions)}",
                _hostingEnvironment.IsDevelopment());

            app.UseMiddleware<GraphQLMiddleware>(new GraphQLSettings
            {
                BuildUserContext = ctx => new GraphQLContext
                {
                    HttpContext = ctx,
                    User = ctx.User,
                    ServiceProvider = ctx.RequestServices,
                },
                ExposeExceptions = exposeExceptions
            });
        }
    }
}
