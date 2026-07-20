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

namespace OrchardCore.Media.Endpoints.Api;

public static class GetMediaFieldItemsEndpoint
{
    public static IEndpointRouteBuilder AddGetMediaFieldItemsEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("api/media/GetMediaFieldItems", HandleAsync)
            .WithName("ApiGetMediaFieldItems")
            .WithTags("MediaApi")
            .DisableAntiforgery()
            .Produces<IEnumerable<FileStoreEntryDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        return builder;
    }

    [Authorize(Policy = MediaApiConstants.AuthorizationPolicyName)]
    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        IAuthorizationService authorizationService,
        IMediaFileStore mediaFileStore,
        IContentTypeProvider contentTypeProvider,
        IFileVersionProvider fileVersionProvider,
        [FromQuery] string[] paths)
    {
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

        var mediaItems = await Task.WhenAll(requestedPaths.Select(async path =>
        {
            var fileEntry = await mediaFileStore.GetFileInfoAsync(path);
            return fileEntry is null ? null : MediaEndpointHelpers.CreateFileResult(fileEntry, httpContext, contentTypeProvider, fileVersionProvider, mediaFileStore);
        }));

        return TypedResults.Ok(mediaItems.Where(static mediaItem => mediaItem is not null).ToArray());
    }
}
