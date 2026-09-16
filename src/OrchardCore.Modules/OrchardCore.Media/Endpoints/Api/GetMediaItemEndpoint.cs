using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticFiles;
using OrchardCore.Media.ViewModels;
using OrchardCore.RemoteManagement;

namespace OrchardCore.Media.Endpoints.Api;

public static class GetMediaItemEndpoint
{
    public static IEndpointRouteBuilder AddGetMediaItemEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapLegacyGet("api/media/GetMediaItem", HandleAsync);

        builder.MapManagementGet("api/media/file", HandleAsync)
            .WithName("ApiGetMediaItem")
            .WithSummary("Shows one media file.")
            .WithDescription("Returns the metadata for a single media file when the caller is allowed to manage the containing folder.")
            .WithCliCommand(new CliOperationMetadata(["media", "files"], "show")
            {
                Capability = MediaApiEndpointConventions.CapabilityName,
                Arguments =
                {
                    new CliArgumentMetadata("path", 0),
                },
                TableColumns =
                {
                    new CliTableColumnMetadata("name", "Name"),
                    new CliTableColumnMetadata("filePath", "Path"),
                    new CliTableColumnMetadata("url", "URL"),
                    new CliTableColumnMetadata("size", "Size"),
                    new CliTableColumnMetadata("mime", "Content type"),
                },
            })
            .Produces<FileStoreEntryDto>(StatusCodes.Status200OK)
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
        [AsParameters] GetMediaFileRequest request)
    {
        var path = request.Path;

        if (!await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMedia)
            || !await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMediaFolder, (object)(path ?? string.Empty)))
        {
            return httpContext.ApiForbidProblem();
        }

        if (string.IsNullOrEmpty(path))
        {
            return httpContext.ApiNotFoundProblem();
        }

        var fileEntry = await mediaFileStore.GetFileInfoAsync(path);

        if (fileEntry == null)
        {
            return httpContext.ApiNotFoundProblem();
        }

        return TypedResults.Ok(MediaEndpointHelpers.CreateFileResult(fileEntry, httpContext, contentTypeProvider, fileVersionProvider, mediaFileStore));
    }
}
