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
        var page = take > 0 ? folders.Skip(skip).Take(take).ToList() : folders;

        // Check HasChildren for the page only (not all folders), considering only accessible sub-folders.
        var hasChildrenTasks = page.Select(async folder =>
        {
            folder.HasChildren = await MediaEndpointHelpers.HasSubDirectoriesAsync(mediaFileStore, authorizationService, httpContext.User, folder.DirectoryPath);
        });
        await Task.WhenAll(hasChildrenTasks);

        return isManagementEndpoint
            ? TypedResults.Ok(new MediaListResponse
            {
                Skip = skip,
                Take = take,
                TotalCount = folders.Count,
                Items = page,
            })
            : TypedResults.Ok(new PaginatedFoldersDto
            {
                Items = page,
                HasMore = take > 0 && skip + page.Count < folders.Count,
            });
    }
}
