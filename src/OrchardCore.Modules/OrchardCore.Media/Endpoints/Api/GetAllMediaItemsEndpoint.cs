using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Options;
using OrchardCore.FileStorage;
using OrchardCore.Media.ViewModels;

namespace OrchardCore.Media.Endpoints.Api;

/// <summary>
/// Returns every media item in the library, walking the whole tree in one request.
/// </summary>
/// <remarks>
/// Deprecated. Nothing in Orchard Core calls it: the Media Gallery loads one directory at a time
/// through <c>GetDirectoryContent</c> and pages the folder tree as you scroll. It is a leftover from the
/// media application the Vue 3 gallery replaced.
/// <para>
/// It is also unbounded by design — no paging, no depth limit, no item cap — so a single request
/// enumerates the entire store, and on a remote store that is one round-trip per directory. Paging it
/// would contradict what it promises, so it is being withdrawn instead. Use
/// <c>GetDirectoryContent</c> to walk the library a directory at a time.
/// </para>
/// </remarks>
public static class GetAllMediaItemsEndpoint
{
    public static IEndpointRouteBuilder AddGetAllMediaItemsEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("api/media/GetAllMediaItems", HandleAsync)
            .WithName("ApiGetAllMediaItems")
            .WithTags("MediaApi")
            .WithSummary("Deprecated. Returns every media item in one unbounded request.")
            .WithDescription(
                "Deprecated and scheduled for removal in a future release. Use GetDirectoryContent to " +
                "walk the library one directory at a time; this endpoint enumerates the whole store in a " +
                "single request.")
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
        IOptions<MediaOptions> options,
        IUserAssetFolderNameProvider userAssetFolderNameProvider,
        string extensions)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMedia)
            || !await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMediaFolder, (object)string.Empty))
        {
            return httpContext.ApiForbidProblem();
        }

        var mediaOptions = options.Value;

        // create default folders if not exist
        if (await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageOwnMedia)
            && await mediaFileStore.GetDirectoryInfoAsync(mediaFileStore.Combine(mediaOptions.AssetsUsersFolder, userAssetFolderNameProvider.GetUserAssetFolderName(httpContext.User))) == null)
        {
            await mediaFileStore.TryCreateDirectoryAsync(mediaFileStore.Combine(mediaOptions.AssetsUsersFolder, userAssetFolderNameProvider.GetUserAssetFolderName(httpContext.User)));
        }

        // RFC 8594: tell callers in a machine-readable way that this endpoint is on its way out.
        httpContext.Response.Headers["Deprecation"] = "true";
        httpContext.Response.Headers["Link"] = "<api/media/GetDirectoryContent>; rel=\"successor-version\"";

        var allowedExtensions = MediaEndpointHelpers.GetRequestedExtensions(mediaOptions, extensions, false);
        var allItems = new List<FileStoreEntryDto>();

        await MediaEndpointHelpers.CollectAllItemsRecursiveAsync(mediaFileStore, authorizationService, httpContext, contentTypeProvider, fileVersionProvider, string.Empty, allowedExtensions, allItems);

        return TypedResults.Ok(allItems);
    }
}
