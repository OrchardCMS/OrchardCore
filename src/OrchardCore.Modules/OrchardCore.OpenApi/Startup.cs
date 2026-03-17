using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.OpenApi.Drivers;
using OrchardCore.OpenApi.Settings;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using Scalar.AspNetCore;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OrchardCore.OpenApi;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.ShouldInclude = operation => operation.HttpMethod != null;
        });

        // Register Swashbuckle Swagger generator. The OAuth2 security scheme is added
        // at Configure time when settings are available.
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "OpenApi V1", Version = "v1" });

            c.DocInclusionPredicate(
                (docName, apiDesc) =>
                {
                    return apiDesc.HttpMethod != null;
                }
            );
        });

        services.AddPermissionProvider<Permissions>();
        services.AddNavigationProvider<AdminMenu>();
        services.AddSiteDisplayDriver<OpenApiSettingsDisplayDriver>();
    }

    public override void Configure(
        IApplicationBuilder app,
        IEndpointRouteBuilder routes,
        IServiceProvider serviceProvider
    )
    {
        var shellSettings = app.ApplicationServices.GetRequiredService<ShellSettings>();
        var prefix = shellSettings?.RequestUrlPrefix;

        // Build the tenant-aware Swagger JSON path once and reuse it.
        var swaggerJson = !string.IsNullOrEmpty(prefix)
            ? $"/{prefix}/swagger/v1/swagger.json"
            : "/swagger/v1/swagger.json";

        // Read settings to determine which UIs are enabled.
        var siteService = app.ApplicationServices.GetRequiredService<ISiteService>();
        var settings = siteService.GetSettingsAsync<OpenApiSettings>()
            .GetAwaiter()
            .GetResult();

        var hasOAuth = !string.IsNullOrEmpty(settings.AuthorizationUrl)
            && !string.IsNullOrEmpty(settings.TokenUrl)
            && !string.IsNullOrEmpty(settings.OAuthClientId);

        // Add the OAuth2 security scheme to the Swagger document when configured.
        if (hasOAuth)
        {
            var scopes = ParseScopes(settings.OAuthScopes);

            var swaggerGenOptions = app.ApplicationServices.GetRequiredService<IOptions<SwaggerGenOptions>>();
            swaggerGenOptions.Value.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri(settings.AuthorizationUrl, UriKind.RelativeOrAbsolute),
                        TokenUrl = new Uri(settings.TokenUrl, UriKind.RelativeOrAbsolute),
                        Scopes = scopes,
                    },
                },
                Scheme = "oauth2",
                Name = "Authorization",
                In = ParameterLocation.Header,
            });
        }

        // Protect all OpenAPI documentation UI endpoints with the ApiViewContent permission.
        // Also block requests to disabled UIs.
        app.Use(
            async (context, next) =>
            {
                var path = context.Request.Path.Value;

                if (path != null)
                {
                    // Always allow the Swagger JSON spec through — ReDoc and Scalar depend on it.
                    var isSwaggerJson = path.StartsWith("/swagger/", StringComparison.OrdinalIgnoreCase)
                        && path.EndsWith(".json", StringComparison.OrdinalIgnoreCase);

                    var isSwaggerUI = !isSwaggerJson
                        && path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase);
                    var isReDoc = path.StartsWith("/redoc", StringComparison.OrdinalIgnoreCase);
                    var isScalar = path.StartsWith("/scalar", StringComparison.OrdinalIgnoreCase);
                    var isOpenApi = path.StartsWith("/openapi", StringComparison.OrdinalIgnoreCase);

                    if (isSwaggerJson || isSwaggerUI || isReDoc || isScalar || isOpenApi)
                    {
                        // Return 404 for disabled UIs (but never block the JSON spec).
                        if ((isSwaggerUI && !settings.EnableSwaggerUI)
                            || (isReDoc && !settings.EnableReDocUI)
                            || (isScalar && !settings.EnableScalarUI))
                        {
                            context.Response.StatusCode = StatusCodes.Status404NotFound;
                            return;
                        }

                        var authorizationService =
                            context.RequestServices.GetRequiredService<IAuthorizationService>();
                        var user = context.User;

                        if (user?.Identity?.IsAuthenticated != true)
                        {
                            context.Response.Redirect("/admin");
                            return;
                        }

                        if (
                            !await authorizationService.AuthorizeAsync(
                                user,
                                OpenApiPermissions.ApiViewContent
                            )
                        )
                        {
                            context.Response.StatusCode = StatusCodes.Status403Forbidden;
                            return;
                        }
                    }
                }

                await next();
            }
        );

        // The OpenAPI JSON and Swagger generator are always registered.
        routes.MapOpenApi();
        app.UseSwagger();

        // Conditionally enable each UI based on settings.
        if (settings.EnableSwaggerUI)
        {
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint(swaggerJson, "OpenApi V1");
                options.DocumentTitle = "OrchardCore OpenAPI Documentation";
                options.ConfigObject.AdditionalItems["withCredentials"] = true;

                if (hasOAuth)
                {
                    options.OAuthClientId(settings.OAuthClientId);
                    options.OAuthAppName("OrchardCore Swagger UI");
                    options.OAuthUsePkce();
                    options.OAuthClientSecret("");
                }
            });
        }

        if (settings.EnableReDocUI)
        {
            app.UseReDoc(options =>
            {
                options.RoutePrefix = "redoc";
                options.SpecUrl = swaggerJson;
                options.DocumentTitle = "OrchardCore OpenAPI Documentation";
            });
        }

        if (settings.EnableScalarUI)
        {
            routes.MapScalarApiReference(options =>
            {
                options
                    .WithOpenApiRoutePattern(swaggerJson)
                    .WithTitle("OrchardCore OpenAPI Documentation")
                    .AddHeadContent("<script>const __originalFetch = window.fetch; window.fetch = (input, init) => __originalFetch(input, { ...init, credentials: 'include' });</script>");

                if (hasOAuth)
                {
                    var scopeList = ParseScopes(settings.OAuthScopes).Keys.ToArray();

                    options.WithOAuth2Authentication(oauth =>
                    {
                        oauth.ClientId = settings.OAuthClientId;
                        oauth.Scopes = scopeList;
                    });
                }
            });
        }
    }

    private static Dictionary<string, string> ParseScopes(string scopes)
    {
        if (string.IsNullOrWhiteSpace(scopes))
        {
            return [];
        }

        return scopes
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToDictionary(
                scope => scope,
                scope => scope[0..1].ToUpperInvariant() + scope[1..]
            );
    }
}
