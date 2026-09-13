using System.Collections.Generic;
using System.IO;
using System.Linq;
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

public static class GetMediaItemsEndpoint
{
    public static IEndpointRouteBuilder AddGetMediaItemsEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapLegacyGet("api/media/GetMediaItems", HandleAsync);

        builder.MapManagementGet("api/media/files", HandleAsync)
            .WithName("ApiGetMediaItems")
            .WithSummary("Lists media files in a folder.")
            .WithDescription("Returns the files that are directly contained in the specified folder, optionally filtered by file extension.")
            .WithCliCommand(new CliOperationMetadata(["media", "files"], "list")
            {
                Capability = MediaApiEndpointConventions.CapabilityName,
                TableColumns =
                {
                    new CliTableColumnMetadata("items[].name", "Name"),
                    new CliTableColumnMetadata("items[].filePath", "Path"),
                    new CliTableColumnMetadata("items[].url", "URL"),
                    new CliTableColumnMetadata("items[].size", "Size"),
                    new CliTableColumnMetadata("items[].mime", "Content type"),
                },
            })
            .Produces<MediaListResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
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
        [AsParameters] BrowseFilesRequest request)
    {
        var path = request.Path;
        var extensions = request.Extensions;
        var isManagementEndpoint = httpContext.GetEndpoint()?.Metadata.GetMetadata<EndpointNameMetadata>()?.EndpointName == "ApiGetMediaItems";
        var skip = request.Skip ?? 0;
        var take = request.Take ?? MediaEndpointHelpers.DefaultTake;

        if (isManagementEndpoint && MediaEndpointHelpers.ValidatePaging(skip, take) is { } pagingError)
        {
            return pagingError;
        }

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
        var allowedExtensions = MediaEndpointHelpers.GetRequestedExtensions(
            options.Value,
            extensions,
            canUploadRestrictedMedia);
        var filterByExtensions = !string.IsNullOrWhiteSpace(extensions);

        var allowed = new List<FileStoreEntryDto>();

        await foreach (var entry in mediaFileStore.GetFilesAsync(path))
        {
            if (!filterByExtensions || allowedExtensions.Contains(Path.GetExtension(entry.Path)))
            {
                allowed.Add(MediaEndpointHelpers.CreateFileResult(entry, httpContext, contentTypeProvider, fileVersionProvider, mediaFileStore));
            }
        }

        allowed.Sort((left, right) => StringComparer.OrdinalIgnoreCase.Compare(left.FilePath, right.FilePath));

        return isManagementEndpoint
            ? TypedResults.Ok(new MediaListResponse
            {
                Skip = skip,
                Take = take,
                TotalCount = allowed.Count,
                Items = allowed.Skip(skip).Take(take).ToArray(),
            })
            : TypedResults.Ok(allowed);
    }
}
