using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OrchardCore.Routing;

namespace OrchardCore.Media.Services;

public class SecureMediaMiddleware
{
    private readonly RequestDelegate _next;
    private readonly PathString _assetsRequestPath;

    public SecureMediaMiddleware(
        RequestDelegate next,
        IOptions<MediaOptions> mediaOptions)
    {
        _next = next;
        _assetsRequestPath = mediaOptions.Value.AssetsRequestPath;
    }

    public Task Invoke(HttpContext context, IAuthorizationService authorizationService, IAuthenticationService authenticationService)
    {
        var validateAssetsRequestPath = context.Request.Path.StartsWithNormalizedSegments(_assetsRequestPath, StringComparison.OrdinalIgnoreCase, out var subPath);
        if (!validateAssetsRequestPath)
        {
            return _next(context);
        }

        return Awaited(context, authorizationService, authenticationService, _next, subPath);

        static async Task Awaited(
            HttpContext context,
            IAuthorizationService authorizationService,
            IAuthenticationService authenticationService,
            RequestDelegate next,
            PathString subPath)
        {
            if (!(context.User.Identity?.IsAuthenticated ?? false))
            {
                // Allow bearer (API) authentication too.
                var authenticateResult = await authenticationService.AuthenticateAsync(context, "Api");

                if (authenticateResult.Succeeded)
                {
                    context.User = authenticateResult.Principal;
                }
            }

            if (await authorizationService.AuthorizeAsync(context.User, MediaPermissions.ViewMedia, (object)subPath.ToString()))
            {
                await next(context);
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
            }
        }
    }
}
