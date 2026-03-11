using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using OrchardCore.Environment.Shell;
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

        // Bearer token authentication is handled by BearerTokenMiddleware in the Configure method

        // Register Swashbuckle Swagger generator to add OAuth2 / OpenID Connect security scheme
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "OpenApi V1", Version = "v1" });

            // Only include actions that have an explicit HTTP method attribute.
            // This prevents helper methods (e.g. CreateFileResult) from causing
            // "Ambiguous HTTP method" errors during document generation.
            // Also exclude the legacy api/media controller — the Gen2 controller supersedes it.
            c.DocInclusionPredicate(
                (docName, apiDesc) =>
                {
                    if (apiDesc.HttpMethod == null)
                    {
                        return false;
                    }

                    // Exclude the legacy Media ApiController (api/media) to avoid
                    // duplicate operation names with MediaGen2ApiController.
                    if (
                        apiDesc.RelativePath != null
                        && apiDesc.RelativePath.StartsWith(
                            "api/media/",
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                    {
                        return false;
                    }

                    return true;
                }
            );

            // Configure OAuth2 / OpenID Connect Authorization Code flow
            var oauthScheme = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri(
                            "/authserver/connect/authorize",
                            UriKind.Relative
                        ),
                        TokenUrl = new Uri("/authserver/connect/token", UriKind.Relative),
                        Scopes = new Dictionary<string, string>
                        {
                            { "openid", "OpenID" },
                            { "profile", "Profile" },
                            { "email", "Email" },
                        },
                    },
                },
                Scheme = "oauth2",
                Name = "Authorization",
                In = ParameterLocation.Header,
            };

            c.AddSecurityDefinition("oauth2", oauthScheme);
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
        var shellSettings = app.ApplicationServices.GetRequiredService<ShellSettings>();
        var prefix = shellSettings?.RequestUrlPrefix;
        var openApiPath = !string.IsNullOrEmpty(prefix)
            ? $"/{prefix}/openapi/v1.json"
            : "/openapi/v1.json";

        // Build the tenant-aware Swagger JSON path once and reuse it.
        var swaggerJson = !string.IsNullOrEmpty(prefix)
            ? $"/{prefix}/swagger/v1/swagger.json"
            : "/swagger/v1/swagger.json";

        routes.MapOpenApi();

        // Serve the Swashbuckle-generated Swagger JSON at /swagger/v1/swagger.json
        app.UseSwagger();

        app.UseSwaggerUI(options =>
        {
            // Point Swagger UI to the Swashbuckle JSON (includes security definitions)
            options.SwaggerEndpoint(swaggerJson, "OpenApi V1");
            options.DocumentTitle = "OrchardCore OpenAPI Documentation";
            options.ConfigObject.AdditionalItems["withCredentials"] = true;

            // Configure OAuth settings for the Swagger UI 'Authorize' dialog.
            options.OAuthClientId("swagger-ui");
            options.OAuthAppName("OrchardCore Swagger UI");
            // Public client: use PKCE instead of client secret
            options.OAuthUsePkce();
            // Explicitly set empty client secret for public clients (prevents prompt in UI)
            options.OAuthClientSecret("");
        });

        app.UseReDoc(options =>
        {
            options.SpecUrl = swaggerJson;
            options.DocumentTitle = "OrchardCore OpenAPI Documentation";
        });

        routes.MapScalarApiReference();
    }
}
