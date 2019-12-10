using System;
using GraphQL;
using GraphQL.Http;
using GraphQL.Validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OrchardCore.Apis.GraphQL.Services;
using OrchardCore.Apis.GraphQL.ValidationRules;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Apis.GraphQL
{
    public class Startup : StartupBase
    {
        private readonly IHostEnvironment _hostingEnvironment;

        public Startup(IHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IDependencyResolver, RequestServicesDependencyResolver>();
            services.AddSingleton<IDocumentExecuter, SerialDocumentExecuter>();
            services.AddSingleton<IDocumentWriter, DocumentWriter>();
            services.AddSingleton<ISchemaFactory, SchemaService>();
            services.AddScoped<IValidationRule, MaxNumberOfResultsValidationRule>();

            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddTransient<INavigationProvider, AdminMenu>();

            services.AddOptions<GraphQLSettings>().Configure<IShellConfiguration>((c, configuration) =>
            {
                var exposeExceptions = configuration.GetValue(
                    $"OrchardCore.Apis.GraphQL:{nameof(GraphQLSettings.ExposeExceptions)}",
                    _hostingEnvironment.IsDevelopment());

                var maxNumberOfResultsValidationMode = configuration.GetValue<MaxNumberOfResultsValidationMode?>($"OrchardCore.Apis.GraphQL:{nameof(GraphQLSettings.MaxNumberOfResultsValidationMode)}")
                                                        ?? MaxNumberOfResultsValidationMode.Default;

                if (maxNumberOfResultsValidationMode == MaxNumberOfResultsValidationMode.Default)
                {
                    maxNumberOfResultsValidationMode = _hostingEnvironment.IsDevelopment() ? MaxNumberOfResultsValidationMode.Enabled : MaxNumberOfResultsValidationMode.Disabled;
                }

                c.BuildUserContext = ctx => new GraphQLContext
                {
                    HttpContext = ctx,
                    User = ctx.User,
                    ServiceProvider = ctx.RequestServices,
                };
                c.ExposeExceptions = exposeExceptions;
                c.MaxDepth = configuration.GetValue<int?>($"OrchardCore.Apis.GraphQL:{nameof(GraphQLSettings.MaxDepth)}") ?? 20;
                c.MaxComplexity = configuration.GetValue<int?>($"OrchardCore.Apis.GraphQL:{nameof(GraphQLSettings.MaxComplexity)}");
                c.FieldImpact = configuration.GetValue<int?>($"OrchardCore.Apis.GraphQL:{nameof(GraphQLSettings.FieldImpact)}");
                c.MaxNumberOfResults = configuration.GetValue<int?>($"OrchardCore.Apis.GraphQL:{nameof(GraphQLSettings.MaxNumberOfResults)}") ?? 1000;
                c.MaxNumberOfResultsValidationMode = maxNumberOfResultsValidationMode;
                c.DefaultNumberOfResults = configuration.GetValue<int?>($"OrchardCore.Apis.GraphQL:{nameof(GraphQLSettings.DefaultNumberOfResults)}") ?? 100;
            });
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            app.UseMiddleware<GraphQLMiddleware>(serviceProvider.GetService<IOptions<GraphQLSettings>>().Value);
        }
    }
}
