using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.FileStorage;
using OrchardCore.Media.ViewModels;

namespace OrchardCore.Media.Endpoints.Api;

public static class MoveMediaListEndpoint
{
    public static IEndpointRouteBuilder AddMoveMediaListEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("api/media/MoveMediaList", HandleAsync)
            .WithName("ApiMoveMediaList")
            .WithTags("MediaApi")
            .DisableAntiforgery()
            .Produces(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return builder;
    }

    [Authorize(AuthenticationSchemes = "Api")]
    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        IAuthorizationService authorizationService,
        IMediaFileStore mediaFileStore,
        IStringLocalizer<MediaApiEndpoints> localizer,
        MoveMedias model)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMedia)
            || !await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMediaFolder, (object)model.sourceFolder)
            || !await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMediaFolder, (object)model.targetFolder))
        {
            return httpContext.ApiForbidProblem();
        }

        if ((model.mediaNames == null) || (model.mediaNames.Length < 1)
            || string.IsNullOrEmpty(model.sourceFolder)
            || string.IsNullOrEmpty(model.targetFolder))
        {
            return httpContext.ApiNotFoundProblem();
        }

        model.sourceFolder = model.sourceFolder == "root" ? string.Empty : model.sourceFolder;
        model.targetFolder = model.targetFolder == "root" ? string.Empty : model.targetFolder;

        var filesOnError = new List<string>();

        foreach (var name in model.mediaNames)
        {
            var sourcePath = mediaFileStore.Combine(model.sourceFolder, name);
            var targetPath = mediaFileStore.Combine(model.targetFolder, name);
            try
            {
                await mediaFileStore.MoveFileAsync(sourcePath, targetPath);
            }
            catch (FileStoreException)
            {
                filesOnError.Add(sourcePath);
            }
        }

        if (filesOnError.Count > 0)
        {
            return httpContext.ApiValidationProblem(detail: localizer["Error when moving files. Maybe they already exist on the target folder? Files on error: {0}", string.Join(",", filesOnError)]);
        }

        return TypedResults.Ok();
    }
}
