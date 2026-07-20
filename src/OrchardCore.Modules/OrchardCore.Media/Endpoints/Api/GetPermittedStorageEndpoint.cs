using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Media.Core.Helpers;
using OrchardCore.Media.ViewModels;

namespace OrchardCore.Media.Endpoints.Api;

public static class GetPermittedStorageEndpoint
{
    public static IEndpointRouteBuilder AddGetPermittedStorageEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("api/media/GetPermittedStorage", HandleAsync)
            .WithName("ApiGetPermittedStorage")
            .WithTags("MediaApi")
            .DisableAntiforgery()
            .Produces<PermittedStorageDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        return builder;
    }

    [Authorize(Policy = MediaApiConstants.AuthorizationPolicyName)]
    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        IAuthorizationService authorizationService,
        IMediaFileStore mediaFileStore,
        IStringLocalizer<MediaApiEndpoints> localizer)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMedia)
            || !await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMediaFolder, (object)string.Empty))
        {
            return httpContext.ApiForbidProblem();
        }

        var bytes = await mediaFileStore.GetPermittedStorageAsync();
        var text = bytes == null ? localizer["Unspecified"] : FileSizeHelpers.FormatAsBytes(bytes.Value);

        return TypedResults.Ok(new PermittedStorageDto { Bytes = bytes, Text = text });
    }
}
