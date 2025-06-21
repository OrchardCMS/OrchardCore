using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Infrastructure.Api;
using OrchardCore.Modules;

namespace OrchardCore.Media.Endpoints.Api;

public class DeleteEndpoints : IEndpoint
{
    public static IEndpointRouteBuilder Map(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete("api/media/folder/{path}", DeleteMediaFolderAsync);
        endpoints.MapDelete("api/media/list/{path}", DeleteMediaAsync);
        endpoints.MapDelete("api/media/{path}", DeleteMediaListAsync);

        return endpoints;
    }

    [Authorize(AuthenticationSchemes = "Api")]
    private static async Task<IResult> DeleteMediaFolderAsync(string path,
        IMediaFileStore mediaFileStore,
        IAuthorizationService authorizationService,
        HttpContext httpContext)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.AccessMediaApi)
            || !await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMediaFolder, (object)path))
        {
            return httpContext.ChallengeOrForbid("Api");
        }

        if (string.IsNullOrEmpty(path))
        {
            return TypedResults.Forbid();
        }

        var mediaFolder = await mediaFileStore.GetDirectoryInfoAsync(path);
        if (mediaFolder != null && !mediaFolder.IsDirectory)
        {
            return TypedResults.Forbid();
        }

        if (await mediaFileStore.TryDeleteDirectoryAsync(path) == false)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok();
    }

    [Authorize(AuthenticationSchemes = "Api")]
    private static async Task<IResult> DeleteMediaListAsync(string[] paths,
    IMediaFileStore mediaFileStore,
    IAuthorizationService authorizationService,
    HttpContext httpContext)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.AccessMediaApi)
            || !await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMedia))
        {
            return httpContext.ChallengeOrForbid("Api");
        }

        if (paths == null)
        {
            return TypedResults.NotFound();
        }

        if (!await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMedia))
        {
            return TypedResults.Forbid();
        }

        foreach (var path in paths)
        {
            if (!await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageAttachedMediaFieldsFolder, (object)path))
            {
                return TypedResults.Forbid();
            }

            if (!await mediaFileStore.TryDeleteFileAsync(path))
            {
                return TypedResults.NotFound();
            }
        }

        return TypedResults.Ok();
    }

    [Authorize(AuthenticationSchemes = "Api")]
    private static async Task<IResult> DeleteMediaAsync(string path,
        IMediaFileStore mediaFileStore,
        IAuthorizationService authorizationService,
        HttpContext httpContext)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.AccessMediaApi)
            || !await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMedia))
        {
            return httpContext.ChallengeOrForbid("Api");
        }

        if (string.IsNullOrEmpty(path))
        {
            return TypedResults.Forbid();
        }

        if (!await mediaFileStore.TryDeleteFileAsync(path))
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok();
    }
}
