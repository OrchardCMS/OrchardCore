using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;
using Scalar.AspNetCore;

namespace OrchardCore.OpenApi;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.ShouldInclude = operation => operation.HttpMethod != null; // Exclude operations without HTTP methods attributes, such as those generated for scalar types.
        });

        services.AddPermissionProvider<Permissions>();
        services.AddNavigationProvider<AdminMenu>();
    }

    public override void Configure(
        IApplicationBuilder app,
        IEndpointRouteBuilder routes,
        IServiceProvider serviceProvider
    )
    {
        routes.MapOpenApi();

        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/openapi/v1.json", "OpenApi V1");
            options.DocumentTitle = "OrchardCore OpenAPI Documentation";
        });

        app.UseReDoc(options =>
        {
            options.SpecUrl = "/openapi/v1.json";
            options.DocumentTitle = "OrchardCore OpenAPI Documentation";
        });

        routes.MapScalarApiReference();
    }
}
