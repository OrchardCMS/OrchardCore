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

public static class GetMediaItemEndpoint
{
    public static IEndpointRouteBuilder AddGetMediaItemEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("api/media/GetMediaItem", HandleAsync)
            .WithName("ApiGetMediaItem")
            .WithTags("MediaApi")
            .DisableAntiforgery()
            .Produces<FileStoreEntryDto>(StatusCodes.Status200OK)
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
        IContentTypeProvider contentTypeProvider,
        IFileVersionProvider fileVersionProvider,
        string path)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMedia))
        {
            return httpContext.ApiForbidProblem();
        }

        if (string.IsNullOrEmpty(path))
        {
            return httpContext.ApiNotFoundProblem();
        }

        var fileEntry = await mediaFileStore.GetFileInfoAsync(path);

        if (fileEntry == null)
        {
            return httpContext.ApiNotFoundProblem();
        }

        return TypedResults.Ok(MediaEndpointHelpers.CreateFileResult(fileEntry, httpContext, contentTypeProvider, fileVersionProvider, mediaFileStore));
    }
}
