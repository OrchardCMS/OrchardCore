using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Media.ViewModels;
using OrchardCore.RemoteManagement;

namespace OrchardCore.Media.Endpoints.Api;

public static class GetFoldersEndpoint
{
    public static IEndpointRouteBuilder AddGetFoldersEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapLegacyGet("api/media/GetFolders", HandleAsync);

        builder.MapManagementGet("api/media/folders", HandleAsync)
            .WithName("ApiGetFolders")
            .WithSummary("Lists media folders in a parent folder.")
            .WithDescription("Returns the folders that are directly contained in the specified parent folder, filtered to the folders the caller is allowed to manage.")
            .WithCliCommand(new CliOperationMetadata(["media", "folders"], "list")
            {
                Capability = MediaApiEndpointConventions.CapabilityName,
                TableColumns =
                {
                    new CliTableColumnMetadata("items[].name", "Name"),
                    new CliTableColumnMetadata("items[].directoryPath", "Path"),
                    new CliTableColumnMetadata("items[].hasChildren", "Has children"),
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
        [AsParameters] BrowseFoldersRequest request)
    {
        var path = request.Path;
        var isManagementEndpoint = httpContext.GetEndpoint()?.Metadata.GetMetadata<EndpointNameMetadata>()?.EndpointName == "ApiGetFolders";
        var skip = request.Skip ?? 0;
        var take = request.Take ?? (isManagementEndpoint ? MediaEndpointHelpers.DefaultTake : 0);

        if (isManagementEndpoint && httpContext.ApiPagingProblem(skip, take) is { } pagingError)
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

        if (!isManagementEndpoint)
        {
            return await GetLegacyPageAsync(httpContext, authorizationService, mediaFileStore, path, skip, take);
        }

        var folders = new List<FileStoreEntryDto>();

        await foreach (var entry in mediaFileStore.GetDirectoriesAsync(path))
        {
            // Only include folders the user is permitted to access.
            // This must happen before counting so that skip/take offsets are
            // calculated against authorized entries only.
            if (!await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMediaFolder, (object)entry.Path))
            {
                continue;
            }

            folders.Add(MediaEndpointHelpers.CreateFolderResult(entry));
        }

        folders.Sort((left, right) => StringComparer.OrdinalIgnoreCase.Compare(left.DirectoryPath, right.DirectoryPath));
        var page = folders.Skip(skip).Take(take).ToList();

        await SetHasChildrenAsync(httpContext, authorizationService, mediaFileStore, page);

        return TypedResults.Ok(new MediaListResponse
        {
            Skip = skip,
            Take = take,
            TotalCount = folders.Count,
            Items = page,
        });
    }

    // Reads the store in its own order and stops as soon as the page is known, without a total count.
    private static async Task<IResult> GetLegacyPageAsync(
        HttpContext httpContext,
        IAuthorizationService authorizationService,
        IMediaFileStore mediaFileStore,
        string path,
        int skip,
        int take)
    {
        var isPaginated = take > 0;
        var page = new List<FileStoreEntryDto>();
        var hasMore = false;
        var authorizedCount = 0;

        await foreach (var entry in mediaFileStore.GetDirectoriesAsync(path))
        {
            // Only include folders the user is permitted to access.
            // This must happen before counting so that skip/take offsets are
            // calculated against authorized entries only.
            if (!await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMediaFolder, (object)entry.Path))
            {
                continue;
            }

            authorizedCount++;

            if (isPaginated)
            {
                // Skip entries before the requested offset.
                if (authorizedCount <= skip)
                {
                    continue;
                }

                // Collect up to 'take' entries for the page.
                if (page.Count < take)
                {
                    page.Add(MediaEndpointHelpers.CreateFolderResult(entry));
                }
                else
                {
                    // We found one more beyond the page, so there are more folders.
                    hasMore = true;
                    break;
                }
            }
            else
            {
                page.Add(MediaEndpointHelpers.CreateFolderResult(entry));
            }
        }

        await SetHasChildrenAsync(httpContext, authorizationService, mediaFileStore, page);

        return TypedResults.Ok(new PaginatedFoldersDto { Items = page, HasMore = hasMore });
    }

    // Checks HasChildren for the page only (not all folders), considering only accessible sub-folders.
    private static Task SetHasChildrenAsync(
        HttpContext httpContext,
        IAuthorizationService authorizationService,
        IMediaFileStore mediaFileStore,
        List<FileStoreEntryDto> page)
        => Task.WhenAll(page.Select(async folder =>
        {
            folder.HasChildren = await MediaEndpointHelpers.HasSubDirectoriesAsync(mediaFileStore, authorizationService, httpContext.User, folder.DirectoryPath);
        }));
}
