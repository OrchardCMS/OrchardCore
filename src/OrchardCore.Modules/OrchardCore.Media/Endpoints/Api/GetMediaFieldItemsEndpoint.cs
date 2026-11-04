using System;
using System.Collections.Generic;
using System.Linq;
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

public static class GetMediaFieldItemsEndpoint
{
    public static IEndpointRouteBuilder AddGetMediaFieldItemsEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapLegacyGet("api/media/GetMediaFieldItems", HandleAsync);

        builder.MapManagementGet("api/media/files/metadata", HandleAsync)
            .WithName("ApiGetMediaFieldItems")
            .WithSummary("Shows metadata for multiple media files.")
            .WithDescription("Returns the metadata for the requested media file paths and omits any paths that no longer resolve to a file.")
            .WithCliCommand(new CliOperationMetadata(["media", "metadata"], "show")
            {
                Capability = MediaApiEndpointConventions.CapabilityName,
                InputMode = CliInputMode.Options,
            })
            .Produces<IEnumerable<FileStoreEntryDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        return builder;
    }

    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        [FromServices] IAuthorizationService authorizationService,
        [FromServices] IMediaFileStore mediaFileStore,
        [FromServices] IContentTypeProvider contentTypeProvider,
        [FromServices] IFileVersionProvider fileVersionProvider,
        [AsParameters] GetMediaFieldItemsRequest request)
    {
        var paths = request.Paths;

        if (!await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMedia))
        {
            return httpContext.ApiForbidProblem();
        }

        var requestedPaths = paths
            ?.Where(static path => !string.IsNullOrWhiteSpace(path))
            .Distinct(StringComparer.Ordinal)
            .ToArray() ?? [];

        if (requestedPaths.Length == 0)
        {
            return TypedResults.Ok(Array.Empty<FileStoreEntryDto>());
        }

        foreach (var path in requestedPaths)
        {
            if (!await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMediaFolder, (object)path))
            {
                return httpContext.ApiForbidProblem();
            }
        }

        var mediaItems = await Task.WhenAll(requestedPaths.Select(async path =>
        {
            var fileEntry = await mediaFileStore.GetFileInfoAsync(path);
            return fileEntry is null ? null : MediaEndpointHelpers.CreateFileResult(fileEntry, httpContext, contentTypeProvider, fileVersionProvider, mediaFileStore);
        }));

        return TypedResults.Ok(mediaItems.Where(static mediaItem => mediaItem is not null).ToArray());
    }
}
