using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using OrchardCore.FileStorage;
using OrchardCore.Media.Services;
using OrchardCore.Media.ViewModels;
using OrchardCore.RemoteManagement;

namespace OrchardCore.Media.Endpoints.Api;

public static class GetAllMediaItemsEndpoint
{
    private static readonly TimeSpan s_snapshotLifetime = TimeSpan.FromSeconds(30);

    public static IEndpointRouteBuilder AddGetAllMediaItemsEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapLegacyGet("api/media/GetAllMediaItems", HandleAsync);

        builder.MapManagementGet("api/media/items", HandleAsync)
            .WithName("ApiGetAllMediaItems")
            .WithSummary("Lists media files recursively.")
            .WithDescription("Returns accessible media files under the media root, optionally filtered by file extension. Use media folders list to list folders.")
            .WithCliCommand(new CliOperationMetadata(["media", "items"], "list")
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
            .ProducesProblem(StatusCodes.Status403Forbidden);

        return builder;
    }

    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        [FromServices] IAuthorizationService authorizationService,
        [FromServices] IMediaFileStore mediaFileStore,
        [FromServices] IContentTypeProvider contentTypeProvider,
        [FromServices] IFileVersionProvider fileVersionProvider,
        [FromServices] IOptions<MediaOptions> options,
        [FromServices] IUserAssetFolderNameProvider userAssetFolderNameProvider,
        [FromServices] IMemoryCache memoryCache,
        [AsParameters] GetAllMediaItemsRequest request)
    {
        var extensions = request.Extensions;
        var isManagementEndpoint = httpContext.GetEndpoint()?.Metadata.GetMetadata<EndpointNameMetadata>()?.EndpointName == "ApiGetAllMediaItems";
        var skip = request.Skip ?? 0;
        var take = request.Take ?? MediaEndpointHelpers.DefaultTake;

        if (isManagementEndpoint && httpContext.ApiPagingProblem(skip, take) is { } pagingError)
        {
            return pagingError;
        }

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

        var canUploadRestrictedMedia = await authorizationService.AuthorizeAsync(
            httpContext.User,
            MediaPermissions.UploadRestrictedMedia);
        var allowedExtensions = MediaEndpointHelpers.GetRequestedExtensions(
            mediaOptions,
            extensions,
            canUploadRestrictedMedia);
        var filterByExtensions = !string.IsNullOrWhiteSpace(extensions);

        async Task<List<FileStoreEntryDto>> WalkAsync()
        {
            var items = new List<FileStoreEntryDto>();

            await MediaEndpointHelpers.CollectAllItemsRecursiveAsync(mediaFileStore, authorizationService, httpContext, contentTypeProvider, fileVersionProvider, string.Empty, allowedExtensions, filterByExtensions, items);

            return items;
        }

        if (!isManagementEndpoint)
        {
            return TypedResults.Ok(await WalkAsync());
        }

        var files = await GetFilesSnapshotAsync(memoryCache, GetSnapshotKey(httpContext, extensions), skip, async () =>
        {
            // Folders are traversed for accessible files but must not occupy file pages.
            var items = await WalkAsync();
            items.RemoveAll(item => item.IsDirectory);
            items.Sort((left, right) => StringComparer.OrdinalIgnoreCase.Compare(left.FilePath, right.FilePath));

            return items;
        });

        return TypedResults.Ok(new MediaListResponse
        {
            Skip = skip,
            Take = take,
            TotalCount = files.Count,
            Items = files.Skip(skip).Take(take).ToArray(),
        });
    }

    /// <summary>
    /// Returns the sorted files of a listing. The first page always walks the store, and the next ones reuse that
    /// walk for a short time, so that paging through a store doesn't walk it for each page. The snapshot is per caller,
    /// since the walk only contains the folders the caller can manage, and the tenant's memory cache keeps tenants apart.
    /// </summary>
    internal static async Task<List<FileStoreEntryDto>> GetFilesSnapshotAsync(IMemoryCache memoryCache, string key, int skip, Func<Task<List<FileStoreEntryDto>>> walkAsync)
    {
        if (key is null)
        {
            return await walkAsync();
        }

        if (skip > 0 && memoryCache.TryGetValue<List<FileStoreEntryDto>>(key, out var snapshot))
        {
            return snapshot;
        }

        var files = await walkAsync();
        memoryCache.Set(key, files, s_snapshotLifetime);

        return files;
    }

    private static string GetSnapshotKey(HttpContext httpContext, string extensions)
    {
        var callerId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? httpContext.User.FindFirstValue("sub");

        // The file URLs in the snapshot are built from the host of the request.
        return callerId is null ? null : $"MediaItemsSnapshot:{callerId}:{httpContext.Request.Host}:{extensions}";
    }
}
