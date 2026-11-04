using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.AspNetCore;
using ModelContextProtocol.Protocol;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;
using OrchardCore.Security;

namespace OrchardCore.RemoteManagement.Mcp;

/// <summary>
/// Registers the tenant's MCP transport and remote management tools.
/// </summary>
[Feature("OrchardCore.RemoteManagement.Mcp")]
public sealed class McpStartup : StartupBase
{
    // Register the challenge header before the tenant's authorization middleware runs.
    /// <inheritdoc />
    public override int ConfigureOrder => int.MinValue;

    /// <inheritdoc />
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddOptions<McpToolOptions>().Configure<IShellConfiguration>((options, configuration) =>
            configuration.GetSection("RemoteManagement:Mcp").Bind(options));
        services.AddSingleton<IHttpContextAccessor, McpHttpContextAccessor>();
        services.AddSingleton<McpEndpointRegistry>();
        services.AddScoped<McpToolCatalog>();
        services.AddScoped<McpToolInvoker>();
        services.AddMcpServer(options =>
        {
            options.ServerInfo = new Implementation { Name = "OrchardCore", Version = Modules.Manifest.ManifestConstants.OrchardCoreVersion };
            options.ScopeRequests = false;
        })
            .WithHttpTransport(options => options.SessionMode = HttpServerSessionMode.Stateless)
            .WithListToolsHandler(async (request, cancellationToken) => new ListToolsResult
            {
                Tools = (await request.Services.GetRequiredService<McpToolCatalog>().GetToolsAsync(cancellationToken))
                    .Select(tool => tool.Tool).ToList(),
            })
            .WithCallToolHandler((request, cancellationToken) =>
                request.Services.GetRequiredService<McpToolInvoker>().InvokeAsync(request.Params, cancellationToken));
    }

    /// <inheritdoc />
    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        serviceProvider.GetRequiredService<McpEndpointRegistry>().DataSources = routes.DataSources;

        app.Use((context, next) =>
        {
            if (context.Request.Path.StartsWithSegments("/mcp"))
            {
                context.Response.Headers.CacheControl = "no-store";
                var origin = context.Request.Headers.Origin;
                if (origin.Count > 0 && (origin.Count != 1 ||
                    !Uri.TryCreate(origin[0], UriKind.Absolute, out var originUri) ||
                    !string.Equals(originUri.GetLeftPart(UriPartial.Authority),
                        $"{context.Request.Scheme}://{context.Request.Host}", StringComparison.OrdinalIgnoreCase)))
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                }

                var metadataUrl = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.PathBase}/.well-known/oauth-protected-resource/mcp";
                context.Response.OnStarting(() =>
                {
                    if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
                    {
                        // Extend the bearer challenge: clients stop at a second one and miss the metadata URL.
                        var challenge = context.Response.Headers.WWWAuthenticate.ToString();
                        var parameters = challenge.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                            ? ", " + challenge["Bearer ".Length..]
                            : string.Empty;
                        context.Response.Headers.WWWAuthenticate = $"Bearer resource_metadata=\"{metadataUrl}\", scope=\"{RemoteManagementConstants.ManagementScope}\"{parameters}";
                    }

                    return Task.CompletedTask;
                });
            }

            return next(context);
        });

        routes.MapGet(".well-known/oauth-protected-resource/mcp", (HttpContext context, RemoteManagementManifestService manifestService) =>
        {
            context.Response.Headers.CacheControl = "no-store";
            var bootstrap = manifestService.CreateBootstrap(context.Request);
            return Results.Json(new
            {
                resource = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.PathBase}/mcp",
                authorization_servers = new[] { bootstrap.Authentication.Authority.AbsoluteUri },
                scopes_supported = new[] { RemoteManagementConstants.ManagementScope },
                bearer_methods_supported = new[] { "header" },
            });
        }).AllowAnonymous().ExcludeFromDescription();

        routes.MapMcp("mcp")
            .RequireAuthorization(ConfigureAccessPolicy)
            .DisableAntiforgery();

        // Stateless servers have no event stream or session. The protocol requires 405 here,
        // which a tenant's catch-all content routes would otherwise turn into 404.
        routes.MapMethods("mcp", [HttpMethods.Get, HttpMethods.Delete], () => Results.StatusCode(StatusCodes.Status405MethodNotAllowed))
            .RequireAuthorization(ConfigureAccessPolicy)
            .ExcludeFromDescription();
    }

    private static void ConfigureAccessPolicy(AuthorizationPolicyBuilder policy) => policy
        .AddAuthenticationSchemes(OrchardCoreConstants.AuthenticationSchemes.Api)
        .RequireAuthenticatedUser()
        .AddRequirements(new PermissionRequirement(RemoteManagementPermissions.AccessRemoteManagement), new McpResourceAudienceRequirement());
}
