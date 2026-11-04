using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using OrchardCore.FileStorage;
using OrchardCore.Media.Services;
using OrchardCore.Media.ViewModels;
using OrchardCore.RemoteManagement;

namespace OrchardCore.Media.Endpoints.Api;

public static class GetDirectoryTreeEndpoint
{
    public static IEndpointRouteBuilder AddGetDirectoryTreeEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapLegacyGet("api/media/GetDirectoryTree", HandleAsync);

        builder.MapManagementGet("api/media/folders/tree", HandleAsync)
            .WithName("ApiGetDirectoryTree")
            .WithSummary("Lists the accessible media folder tree.")
            .WithDescription("Returns the media folder hierarchy that the caller is allowed to browse. Inaccessible folders are omitted from the tree.")
            .WithCliCommand(new CliOperationMetadata(["media", "folders"], "tree")
            {
                Capability = MediaApiEndpointConventions.CapabilityName,
            })
            .Produces<DirectoryTreeNodeDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        return builder;
    }

    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        [FromServices] IAuthorizationService authorizationService,
        [FromServices] IMediaFileStore mediaFileStore,
        [FromServices] IOptions<MediaOptions> options,
        [FromServices] IUserAssetFolderNameProvider userAssetFolderNameProvider,
        [FromServices] MediaDirectoryTreeCache directoryTreeCache)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMedia)
            || !await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMediaFolder, (object)string.Empty))
        {
            return httpContext.ApiForbidProblem();
        }

        var mediaOptions = options.Value;

        // Create default user folder if needed.
        if (await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageOwnMedia)
            && await mediaFileStore.GetDirectoryInfoAsync(mediaFileStore.Combine(mediaOptions.AssetsUsersFolder, userAssetFolderNameProvider.GetUserAssetFolderName(httpContext.User))) == null)
        {
            await mediaFileStore.TryCreateDirectoryAsync(mediaFileStore.Combine(mediaOptions.AssetsUsersFolder, userAssetFolderNameProvider.GetUserAssetFolderName(httpContext.User)));
        }

        var cached = await directoryTreeCache.GetTreeAsync();

        return TypedResults.Ok(await MediaEndpointHelpers.ToDtoAsync(authorizationService, httpContext.User, cached));
    }
}
