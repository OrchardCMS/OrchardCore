using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticFiles;
using OrchardCore.Media.Services;
using OrchardCore.Media.ViewModels;
using OrchardCore.RemoteManagement;

namespace OrchardCore.Media.Endpoints.Api;

public static class GetTusFileInfoEndpoint
{
    public static IEndpointRouteBuilder AddGetTusFileInfoEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapLegacyGet("api/media/TusFileInfo/{uploadId}", HandleAsync);

        builder.MapManagementGet("api/media/uploads/{uploadId}", HandleAsync)
            .WithName("ApiGetTusFileInfo")
            .WithSummary("Shows the file metadata for a completed resumable upload.")
            .WithDescription("Returns the created media file for a completed TUS upload and removes the temporary upload metadata once it has been read.")
            .WithCliCommand(new CliOperationMetadata(["media", "uploads"], "show")
            {
                Capability = MediaApiEndpointConventions.CapabilityName,
                Arguments =
                {
                    new CliArgumentMetadata("uploadId", 0),
                },
                TableColumns =
                {
                    new CliTableColumnMetadata("name", "Name"),
                    new CliTableColumnMetadata("filePath", "Path"),
                    new CliTableColumnMetadata("url", "URL"),
                    new CliTableColumnMetadata("size", "Size"),
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
        [FromServices] DistributedTusUploadMetadataStore tusMetadataStore,
        string uploadId)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMedia))
        {
            return httpContext.ApiForbidProblem();
        }

        var entry = await tusMetadataStore.GetAsync(uploadId);

        if (entry == null || string.IsNullOrEmpty(entry.MediaFilePath))
        {
            return httpContext.ApiNotFoundProblem();
        }

        var fileInfo = await mediaFileStore.GetFileInfoAsync(entry.MediaFilePath);

        if (fileInfo == null)
        {
            return httpContext.ApiNotFoundProblem();
        }

        // Remove the entry now that the client has retrieved it.
        await tusMetadataStore.RemoveAsync(uploadId);

        return TypedResults.Ok(MediaEndpointHelpers.CreateFileResult(fileInfo, httpContext, contentTypeProvider, fileVersionProvider, mediaFileStore));
    }
}
