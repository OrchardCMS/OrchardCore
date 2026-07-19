using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Options;
using OrchardCore.Media.ViewModels;

namespace OrchardCore.Media.Endpoints.Api;

public static class GetDirectoryContentEndpoint
{
    public static IEndpointRouteBuilder AddGetDirectoryContentEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("api/media/GetDirectoryContent", HandleAsync)
            .WithName("ApiGetDirectoryContent")
            .WithTags("MediaApi")
            .DisableAntiforgery()
            .Produces<DirectoryContentDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return builder;
    }

    [Authorize(AuthenticationSchemes = "Api")]
    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        IAuthorizationService authorizationService,
        IMediaFileStore mediaFileStore,
        IContentTypeProvider contentTypeProvider,
        IFileVersionProvider fileVersionProvider,
        IOptions<MediaOptions> options,
        string path,
        string extensions)
    {
        if (string.IsNullOrEmpty(path))
        {
            path = string.Empty;
        }

        if (!await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMedia))
        {
            return httpContext.ApiForbidProblem();
        }

        // Only check directory existence for non-root paths (root always exists).
        if (path.Length > 0 && await mediaFileStore.GetDirectoryInfoAsync(path) == null)
        {
            return httpContext.ApiNotFoundProblem();
        }

        // Fetch folders and files concurrently.
        var foldersTask = MediaEndpointHelpers.GetDirectoryFoldersAsync(mediaFileStore, path);
        var filesTask = MediaEndpointHelpers.GetDirectoryFilesAsync(mediaFileStore, httpContext, contentTypeProvider, fileVersionProvider, options.Value, path, extensions);

        await Task.WhenAll(foldersTask, filesTask);

        return TypedResults.Ok(new DirectoryContentDto
        {
            Folders = foldersTask.Result,
            Files = filesTask.Result,
        });
    }
}
