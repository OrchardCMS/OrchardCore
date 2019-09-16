using System;
using GraphQL;
using GraphQL.Http;
using GraphQL.Validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
        private readonly IShellConfiguration _configuration;
        private readonly IHostEnvironment _hostingEnvironment;

        public Startup(IShellConfiguration configuration,
            IHostEnvironment hostingEnvironment)
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

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var exposeExceptions = _configuration.GetValue(
                $"OrchardCore.Apis.GraphQL:{nameof(GraphQLSettings.ExposeExceptions)}",
                _hostingEnvironment.IsDevelopment());

            var maxNumberOfResultsValidationMode = _configuration.GetValue<MaxNumberOfResultsValidationMode?>($"OrchardCore.Apis.GraphQL:{nameof(GraphQLSettings.MaxNumberOfResultsValidationMode)}")
                                                    ?? MaxNumberOfResultsValidationMode.Environment;

            if (maxNumberOfResultsValidationMode == MaxNumberOfResultsValidationMode.Environment)
            {
                maxNumberOfResultsValidationMode = _hostingEnvironment.IsDevelopment() ? MaxNumberOfResultsValidationMode.Debug : MaxNumberOfResultsValidationMode.Release;
            }

            app.UseMiddleware<GraphQLMiddleware>(new GraphQLSettings
            {
                BuildUserContext = ctx => new GraphQLContext
                {
                    HttpContext = ctx,
                    User = ctx.User,
                    ServiceProvider = ctx.RequestServices,
                },
                ExposeExceptions = exposeExceptions,
                ValidationRules = serviceProvider.GetServices<IValidationRule>(),
                MaxDepth = _configuration.GetValue<int?>($"OrchardCore.Apis.GraphQL:{nameof(GraphQLSettings.MaxDepth)}") ?? 20,
                MaxComplexity = _configuration.GetValue<int?>($"OrchardCore.Apis.GraphQL:{nameof(GraphQLSettings.MaxComplexity)}"),
                FieldImpact = _configuration.GetValue<int?>($"OrchardCore.Apis.GraphQL:{nameof(GraphQLSettings.FieldImpact)}"),
                MaxNumberOfResults = _configuration.GetValue<int?>($"OrchardCore.Apis.GraphQL:{nameof(GraphQLSettings.MaxNumberOfResults)}") ?? 1000,
                MaxNumberOfResultsValidationMode = maxNumberOfResultsValidationMode
            });
        }
    }
}
