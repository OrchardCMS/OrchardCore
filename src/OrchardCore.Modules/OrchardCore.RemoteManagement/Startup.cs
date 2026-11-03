using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using OrchardCore.Security.Permissions;

namespace OrchardCore.RemoteManagement;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddOpenApi();
        services.AddPermissionProvider<Permissions>();
        services.AddScoped<IAuthorizationHandler, ManagementScopeAuthorizationHandler>();
        services.AddScoped<RemoteManagementManifestService>();
        services.AddSingleton<RemoteManagementApiRevision>();
        services.AddSingleton<IRemoteManagementCapabilityProvider, DefaultCapabilityProvider>();
    }

    public override void Configure(
        IApplicationBuilder app,
        IEndpointRouteBuilder routes,
        IServiceProvider serviceProvider)
    {
        var revision = serviceProvider.GetRequiredService<RemoteManagementApiRevision>();
        app.Use((context, next) =>
        {
            if (context.Request.Path.StartsWithSegments("/api") || context.Request.Path.StartsWithSegments("/openapi"))
            {
                context.Response.OnStarting(() =>
                {
                    context.Response.Headers[RemoteManagementConstants.ApiRevisionHeaderName] = revision.Value;
                    return Task.CompletedTask;
                });
            }

            return next(context);
        });

        routes.MapMethods("api/management/manifest", [HttpMethods.Head], HandleManifestRevisionAsync)
            .ExcludeFromDescription();

        routes.MapGet(RemoteManagementConstants.BootstrapPath, HandleBootstrap)
            .AllowAnonymous()
            .ExcludeFromDescription();

        routes.MapGet("api/management/manifest", HandleManifestAsync)
            .WithName("ApiGetRemoteManagementManifest")
            .WithTags("RemoteManagement");
    }

    [Authorize(AuthenticationSchemes = OrchardCoreConstants.AuthenticationSchemes.Api)]
    private static async Task<IResult> HandleManifestRevisionAsync(
        HttpContext context,
        IAuthorizationService authorizationService)
    {
        context.Response.Headers.CacheControl = "no-store";
        if (!await authorizationService.AuthorizeAsync(
            context.User,
            RemoteManagementPermissions.AccessRemoteManagement))
        {
            return context.ChallengeOrForbid(OrchardCoreConstants.AuthenticationSchemes.Api);
        }

        return TypedResults.Ok();
    }

    internal static Ok<RemoteManagementManifest> HandleBootstrap(
        HttpRequest request,
        RemoteManagementManifestService manifestService)
    {
        // The document reflects the host of the request, so a shared cache must not serve it to another host.
        request.HttpContext.Response.Headers.CacheControl = "no-store";

        return TypedResults.Ok(manifestService.CreateBootstrap(request));
    }

    [Authorize(AuthenticationSchemes = OrchardCoreConstants.AuthenticationSchemes.Api)]
    private static async Task<IResult> HandleManifestAsync(
        HttpContext context,
        IAuthorizationService authorizationService,
        RemoteManagementManifestService manifestService)
    {
        if (!await authorizationService.AuthorizeAsync(
            context.User,
            RemoteManagementPermissions.AccessRemoteManagement))
        {
            return context.ChallengeOrForbid(OrchardCoreConstants.AuthenticationSchemes.Api);
        }

        return TypedResults.Ok(await manifestService.CreateAuthenticatedAsync(context.Request));
    }
}
