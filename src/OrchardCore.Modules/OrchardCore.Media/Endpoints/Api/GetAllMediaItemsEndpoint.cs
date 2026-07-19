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

public static class GetAllMediaItemsEndpoint
{
    public static IEndpointRouteBuilder AddGetAllMediaItemsEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("api/media/GetAllMediaItems", HandleAsync)
            .WithName("ApiGetAllMediaItems")
            .WithTags("MediaApi")
            .DisableAntiforgery()
            .Produces<IEnumerable<FileStoreEntryDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

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
        IUserAssetFolderNameProvider userAssetFolderNameProvider,
        string extensions)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMedia))
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

        var allowedExtensions = MediaEndpointHelpers.GetRequestedExtensions(mediaOptions, extensions, false);
        var allItems = new List<FileStoreEntryDto>();

        await MediaEndpointHelpers.CollectAllItemsRecursiveAsync(mediaFileStore, httpContext, contentTypeProvider, fileVersionProvider, string.Empty, allowedExtensions, allItems);

        return TypedResults.Ok(allItems);
    }
}
