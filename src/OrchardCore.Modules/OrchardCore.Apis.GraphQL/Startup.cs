using GraphQL;
using GraphQL.DataLoader;
using GraphQL.Execution;
using GraphQL.Validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OrchardCore.Apis.GraphQL.Json;
using OrchardCore.Apis.GraphQL.Services;
using OrchardCore.Apis.GraphQL.ValidationRules;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Json;
using OrchardCore.Json.Extensions;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Apis.GraphQL;

public sealed class Startup : StartupBase
{
    private readonly IHostEnvironment _hostingEnvironment;

    public Startup(IHostEnvironment hostingEnvironment)
    {
        _hostingEnvironment = hostingEnvironment;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IDocumentExecuter, DocumentExecuter>();
        services.AddSingleton<IDataLoaderContextAccessor, DataLoaderContextAccessor>();
        services.AddSingleton<IDocumentExecutionListener, DataLoaderDocumentListener>();

        services.AddSingleton<ISchemaFactory, SchemaService>();
        services.AddScoped<IValidationRule, MaxNumberOfResultsValidationRule>();
        services.AddScoped<IValidationRule, RequiresPermissionValidationRule>();

        services.AddSingleton<IErrorInfoProvider>(services =>
        {
            var settings = services.GetRequiredService<IOptions<GraphQLSettings>>();
            return new ErrorInfoProvider(new ErrorInfoProviderOptions { ExposeExceptionDetails = settings.Value.ExposeExceptions });
        });

        services.AddPermissionProvider<Permissions>();
        services.AddNavigationProvider<AdminMenu>();
        services.AddSingleton<GraphQLMiddleware>();

        services.AddGraphQL(builder => builder.AddSystemTextJson((options, sp) =>
        {
            // Common types of converters are already configured in the assembly "GraphQL.SystemTextJson".
            options.Converters.Add(GraphQLNamedQueryRequestJsonConverter.Instance);

            var documentJsonSerializerOptions = sp.GetRequiredService<IOptions<DocumentJsonSerializerOptions>>().Value;
            options.Merge(documentJsonSerializerOptions.SerializerOptions);
        }));

        services.AddOptions<GraphQLSettings>().Configure<IShellConfiguration>((c, configuration) =>
        {
            var exposeExceptions = configuration.GetValue(
                $"OrchardCore_Apis_GraphQL:{nameof(GraphQLSettings.ExposeExceptions)}",
                _hostingEnvironment.IsDevelopment());

            var maxNumberOfResultsValidationMode = configuration.GetValue<MaxNumberOfResultsValidationMode?>($"OrchardCore_Apis_GraphQL:{nameof(GraphQLSettings.MaxNumberOfResultsValidationMode)}")
                                                    ?? MaxNumberOfResultsValidationMode.Default;

            if (maxNumberOfResultsValidationMode == MaxNumberOfResultsValidationMode.Default)
            {
                maxNumberOfResultsValidationMode = _hostingEnvironment.IsDevelopment() ? MaxNumberOfResultsValidationMode.Enabled : MaxNumberOfResultsValidationMode.Disabled;
            }

            c.BuildUserContext = ctx => new GraphQLUserContext
            {
                User = ctx.User,
            };
            c.ExposeExceptions = exposeExceptions;
            c.MaxDepth = configuration.GetValue<int?>($"OrchardCore_Apis_GraphQL:{nameof(GraphQLSettings.MaxDepth)}") ?? 100;
            c.MaxComplexity = configuration.GetValue<int?>($"OrchardCore_Apis_GraphQL:{nameof(GraphQLSettings.MaxComplexity)}");
            c.FieldImpact = configuration.GetValue<double?>($"OrchardCore_Apis_GraphQL:{nameof(GraphQLSettings.FieldImpact)}");
            c.MaxNumberOfResults = configuration.GetValue<int?>($"OrchardCore_Apis_GraphQL:{nameof(GraphQLSettings.MaxNumberOfResults)}") ?? 1000;
            c.MaxNumberOfResultsValidationMode = maxNumberOfResultsValidationMode;
            c.DefaultNumberOfResults = configuration.GetValue<int?>($"OrchardCore_Apis_GraphQL:{nameof(GraphQLSettings.DefaultNumberOfResults)}") ?? 100;
        });
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        app.UseMiddleware<GraphQLMiddleware>();
    }
}
