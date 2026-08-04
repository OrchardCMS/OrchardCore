using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Media.ViewModels;

namespace OrchardCore.Media.Endpoints.Api;

public static class GetFoldersEndpoint
{
    public static IEndpointRouteBuilder AddGetFoldersEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("api/media/GetFolders", HandleAsync)
            .WithName("ApiGetFolders")
            .WithTags("MediaApi")
            .DisableAntiforgery()
            .Produces<PaginatedFoldersDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return builder;
    }

    [Authorize(Policy = MediaApiConstants.AuthorizationPolicyName)]
    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        IAuthorizationService authorizationService,
        IMediaFileStore mediaFileStore,
        string path,
        int skip = 0,
        int take = 0)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMedia))
        {
            return httpContext.ApiForbidProblem();
        }

        if (string.IsNullOrEmpty(path))
        {
            path = string.Empty;
        }

        // Only check directory existence for non-root paths (root always exists).
        if (path.Length > 0 && await mediaFileStore.GetDirectoryInfoAsync(path) == null)
        {
            return httpContext.ApiNotFoundProblem();
        }

        var isPaginated = take > 0;
        var page = new List<FileStoreEntryDto>();
        var hasMore = false;
        var authorizedCount = 0;

        await foreach (var entry in mediaFileStore.GetDirectoriesAsync(path))
        {
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
                    // We found one more beyond the page — there are more folders.
                    hasMore = true;
                    break;
                }
            }
            else
            {
                page.Add(MediaEndpointHelpers.CreateFolderResult(entry));
            }
        }

        // Check HasChildren for the page only (not all folders).
        var hasChildrenTasks = page.Select(async folder =>
        {
            folder.HasChildren = await MediaEndpointHelpers.HasSubDirectoriesAsync(mediaFileStore, folder.DirectoryPath);
        });
        await Task.WhenAll(hasChildrenTasks);

        return TypedResults.Ok(new PaginatedFoldersDto { Items = page, HasMore = hasMore });
    }
}
