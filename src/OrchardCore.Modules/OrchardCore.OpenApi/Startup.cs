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
using Swashbuckle.AspNetCore.Swagger;
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

        services.AddHttpClient();
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

        // Add the server URL to the Swagger document so Scalar knows where to send requests.
        var swaggerGenOptions = app.ApplicationServices.GetRequiredService<IOptions<SwaggerGenOptions>>();
        var basePath = !string.IsNullOrEmpty(prefix) ? $"/{prefix}" : "/";
        swaggerGenOptions.Value.AddServer(new OpenApiServer { Url = basePath });

        // Add security schemes to the Swagger document based on the selected authentication type.
        ConfigureSecurityScheme(app, settings);

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

        var hasOAuth = settings.AuthenticationType != OpenApiAuthenticationType.None
            && !string.IsNullOrEmpty(settings.TokenUrl);

        // Conditionally enable each UI based on settings.
        if (settings.EnableSwaggerUI)
        {
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint(swaggerJson, "OpenApi V1");
                options.DocumentTitle = "OrchardCore OpenAPI Documentation";

                if (!hasOAuth)
                {
                    // Cookie auth: send credentials with API requests so the session cookie is included.
                    options.ConfigObject.AdditionalItems["withCredentials"] = true;
                }

                if (hasOAuth && !string.IsNullOrEmpty(settings.OAuthClientId))
                {
                    options.OAuthClientId(settings.OAuthClientId);
                    options.OAuthAppName("OrchardCore Swagger UI");

                    if (settings.AuthenticationType == OpenApiAuthenticationType.AuthorizationCodePkce)
                    {
                        options.OAuthUsePkce();
                    }

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
                    // Disable Scalar's default external proxy so all requests (token + API)
                    // go directly from the browser to this server.
                    .WithProxyUrl("");

                if (!hasOAuth)
                {
                    // Cookie auth: override fetch to include credentials so the session cookie is sent.
                    options.AddHeadContent("<script>const __originalFetch = window.fetch; window.fetch = (input, init) => __originalFetch(input, { ...init, credentials: 'include' });</script>");
                }

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

    private static void ConfigureSecurityScheme(IApplicationBuilder app, OpenApiSettings settings)
    {
        if (settings.AuthenticationType == OpenApiAuthenticationType.None
            || string.IsNullOrEmpty(settings.TokenUrl))
        {
            return;
        }

        var scopes = ParseScopes(settings.OAuthScopes);
        var swaggerGenOptions = app.ApplicationServices.GetRequiredService<IOptions<SwaggerGenOptions>>();
        var tokenUrl = new Uri(settings.TokenUrl, UriKind.RelativeOrAbsolute);

        var flows = new OpenApiOAuthFlows();

        switch (settings.AuthenticationType)
        {
            case OpenApiAuthenticationType.AuthorizationCodePkce:
                if (string.IsNullOrEmpty(settings.AuthorizationUrl))
                {
                    return;
                }

                flows.AuthorizationCode = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = new Uri(settings.AuthorizationUrl, UriKind.RelativeOrAbsolute),
                    TokenUrl = tokenUrl,
                    Scopes = scopes,
                };
                break;

            case OpenApiAuthenticationType.ClientCredentials:
                flows.ClientCredentials = new OpenApiOAuthFlow
                {
                    TokenUrl = tokenUrl,
                    Scopes = scopes,
                };
                break;
        }

        swaggerGenOptions.Value.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Flows = flows,
            Scheme = "oauth2",
            Name = "Authorization",
            In = ParameterLocation.Header,
        });

        // Apply the security scheme globally so that Scalar (and Swagger) automatically
        // attach the Bearer token to every API request after authorization.
        var scopeKeys = scopes.Keys.ToList();
        swaggerGenOptions.Value.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecuritySchemeReference("oauth2", doc),
                scopeKeys
            },
        });
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

/// <summary>
/// Runs before <c>UseAuthentication()</c> to strip duplicate client credentials
/// from token requests. Scalar sends credentials in both the request body and the
/// Authorization header, which OpenIddict rejects per RFC 6749 §2.3.
/// </summary>
public sealed class TokenRequestStartup : StartupBase
{
    public override int ConfigureOrder => OrchardCoreConstants.ConfigureOrder.Authentication - 10;

    public override void Configure(
        IApplicationBuilder app,
        IEndpointRouteBuilder routes,
        IServiceProvider serviceProvider)
    {
        app.Use(async (context, next) =>
        {
            var path = context.Request.Path.Value;
            if (path != null
                && path.EndsWith("/connect/token", StringComparison.OrdinalIgnoreCase)
                && HttpMethods.IsPost(context.Request.Method)
                && context.Request.HasFormContentType
                && !string.IsNullOrEmpty(context.Request.Headers.Authorization))
            {
                // Only strip the Authorization header when client credentials are also
                // present in the form body (Scalar sends both, causing OpenIddict to
                // reject with "Multiple client credentials"). Swagger sends credentials
                // only in the header, so we must not strip it in that case.
                var form = await context.Request.ReadFormAsync();
                if (form.ContainsKey("client_id"))
                {
                    context.Request.Headers.Remove("Authorization");
                }
            }

            await next();
        });
    }
}
