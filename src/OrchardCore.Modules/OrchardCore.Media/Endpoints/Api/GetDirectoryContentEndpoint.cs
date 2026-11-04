using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Options;
using OrchardCore.Media.Services;
using OrchardCore.Media.ViewModels;
using OrchardCore.RemoteManagement;

namespace OrchardCore.Media.Endpoints.Api;

public static class GetDirectoryContentEndpoint
{
    public static IEndpointRouteBuilder AddGetDirectoryContentEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapLegacyGet("api/media/GetDirectoryContent", HandleAsync);

        builder.MapManagementGet("api/media/directories/content", HandleAsync)
            .WithName("ApiGetDirectoryContent")
            .WithSummary("Lists the folders and files in a folder.")
            .WithDescription("Returns both child folders and files for a media folder in a single response, filtered to the entries the caller can access.")
            .WithCliCommand(new CliOperationMetadata(["media", "directories"], "show")
            {
                Capability = MediaApiEndpointConventions.CapabilityName,
            })
            .Produces<DirectoryContentDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return builder;
    }

    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        [FromServices] IAuthorizationService authorizationService,
        [FromServices] IMediaFileStore mediaFileStore,
        [FromServices] IContentTypeProvider contentTypeProvider,
        [FromServices] IFileVersionProvider fileVersionProvider,
        [FromServices] IOptions<MediaOptions> options,
        [AsParameters] BrowseDirectoryContentRequest request)
    {
        var path = request.Path;
        var extensions = request.Extensions;

        if (string.IsNullOrEmpty(path))
        {
            path = string.Empty;
        }

        if (!await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMedia)
            || !await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMediaFolder, (object)path))
        {
            return httpContext.ApiForbidProblem();
        }

        // Only check directory existence for non-root paths (root always exists).
        if (path.Length > 0 && await mediaFileStore.GetDirectoryInfoAsync(path) == null)
        {
            return httpContext.ApiNotFoundProblem();
        }

        var canUploadRestrictedMedia = await authorizationService.AuthorizeAsync(
            httpContext.User,
            MediaPermissions.UploadRestrictedMedia);

        // Fetch folders and files concurrently.
        var foldersTask = MediaEndpointHelpers.GetDirectoryFoldersAsync(mediaFileStore, authorizationService, httpContext.User, path);
        var filesTask = MediaEndpointHelpers.GetDirectoryFilesAsync(
            mediaFileStore,
            httpContext,
            contentTypeProvider,
            fileVersionProvider,
            options.Value,
            canUploadRestrictedMedia,
            path,
            extensions);

        await Task.WhenAll(foldersTask, filesTask);

        return TypedResults.Ok(new DirectoryContentDto
        {
            Folders = foldersTask.Result,
            Files = filesTask.Result,
        });
    }
}
