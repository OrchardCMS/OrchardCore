using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.FileStorage;
using OrchardCore.Media.Services;
using OrchardCore.Media.ViewModels;
using OrchardCore.RemoteManagement;

namespace OrchardCore.Media.Endpoints.Api;

public static class CreateFolderEndpoint
{
    public static IEndpointRouteBuilder AddCreateFolderEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapLegacyPost("api/media/CreateFolder", HandleLegacyAsync)
            .AddEndpointFilter<MediaApiAntiforgeryEndpointFilter>();

        builder.MapManagementPost("api/media/folders", HandleAsync)
            .WithName("ApiCreateFolder")
            .WithSummary("Creates a media folder.")
            .WithDescription("Creates a folder under the specified parent media folder after validating the name and folder permissions.")
            .WithCliCommand(new CliOperationMetadata(["media", "folders"], "create")
            {
                Capability = MediaApiEndpointConventions.CapabilityName,
                InputMode = CliInputMode.Options,
                TableColumns =
                {
                    new CliTableColumnMetadata("folder.name", "Name"),
                    new CliTableColumnMetadata("folder.directoryPath", "Path"),
                },
            })
            .Accepts<CreateFolderRequest>("application/json")
            .Produces<CreateFolderResultDto>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        return builder;
    }

    private static Task<IResult> HandleLegacyAsync(
        HttpContext httpContext,
        [FromServices] IAuthorizationService authorizationService,
        [FromServices] IMediaFileStore mediaFileStore,
        [FromServices] IMediaNameNormalizerService mediaNameNormalizerService,
        [FromServices] IOptions<MediaOptions> options,
        [FromServices] AttachedMediaFieldFileService attachedMediaFieldFileService,
        [FromServices] MediaDirectoryTreeCache directoryTreeCache,
        [FromServices] IStringLocalizer<MediaApiEndpoints> localizer,
        string path,
        string name)
        => HandleLegacyResultAsync(httpContext, authorizationService, mediaFileStore, mediaNameNormalizerService, options, attachedMediaFieldFileService, directoryTreeCache, localizer, path, name);

    private static Task<IResult> HandleAsync(
        HttpContext httpContext,
        [FromServices] IAuthorizationService authorizationService,
        [FromServices] IMediaFileStore mediaFileStore,
        [FromServices] IMediaNameNormalizerService mediaNameNormalizerService,
        [FromServices] IOptions<MediaOptions> options,
        [FromServices] AttachedMediaFieldFileService attachedMediaFieldFileService,
        [FromServices] MediaDirectoryTreeCache directoryTreeCache,
        [FromServices] IStringLocalizer<MediaApiEndpoints> localizer,
        [FromBody] CreateFolderRequest request)
        => HandleManagementAsync(httpContext, authorizationService, mediaFileStore, mediaNameNormalizerService, options, attachedMediaFieldFileService, directoryTreeCache, localizer, request.Path, request.Name);

    private static async Task<IResult> HandleLegacyResultAsync(
        HttpContext httpContext,
        IAuthorizationService authorizationService,
        IMediaFileStore mediaFileStore,
        IMediaNameNormalizerService mediaNameNormalizerService,
        IOptions<MediaOptions> options,
        AttachedMediaFieldFileService attachedMediaFieldFileService,
        MediaDirectoryTreeCache directoryTreeCache,
        IStringLocalizer<MediaApiEndpoints> localizer,
        string path,
        string name)
    {
        var (result, folder) = await CreateFolderAsync(httpContext, authorizationService, mediaFileStore, mediaNameNormalizerService, options, attachedMediaFieldFileService, directoryTreeCache, localizer, path, name);

        return result ?? TypedResults.Ok(MediaEndpointHelpers.CreateFolderResult(folder));
    }

    private static async Task<IResult> HandleManagementAsync(
        HttpContext httpContext,
        IAuthorizationService authorizationService,
        IMediaFileStore mediaFileStore,
        IMediaNameNormalizerService mediaNameNormalizerService,
        IOptions<MediaOptions> options,
        AttachedMediaFieldFileService attachedMediaFieldFileService,
        MediaDirectoryTreeCache directoryTreeCache,
        IStringLocalizer<MediaApiEndpoints> localizer,
        string path,
        string name)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMedia))
        {
            return httpContext.ApiForbidProblem();
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return httpContext.ApiValidationProblem(detail: localizer["A folder name is required."]);
        }

        var (result, folder) = await CreateFolderAsync(httpContext, authorizationService, mediaFileStore, mediaNameNormalizerService, options, attachedMediaFieldFileService, directoryTreeCache, localizer, path, name);

        return result ?? TypedResults.Ok(new CreateFolderResultDto
        {
            Folder = MediaEndpointHelpers.CreateFolderResult(folder),
        });
    }

    private static async Task<(IResult Result, IFileStoreEntry Folder)> CreateFolderAsync(
        HttpContext httpContext,
        IAuthorizationService authorizationService,
        IMediaFileStore mediaFileStore,
        IMediaNameNormalizerService mediaNameNormalizerService,
        IOptions<MediaOptions> options,
        AttachedMediaFieldFileService attachedMediaFieldFileService,
        MediaDirectoryTreeCache directoryTreeCache,
        IStringLocalizer<MediaApiEndpoints> localizer,
        string path,
        string name)
    {
        if (string.IsNullOrEmpty(path))
        {
            path = string.Empty;
        }

        name = mediaNameNormalizerService.NormalizeFolderName(name);

        if (MediaEndpointHelpers.InvalidFolderNameCharacters.Any(invalidChar => name.Contains(invalidChar)))
        {
            return (httpContext.ApiValidationProblem(detail: localizer["Cannot create folder because the folder name contains invalid characters"]), null);
        }

        if (MediaEndpointHelpers.IsSpecialFolder(options.Value, attachedMediaFieldFileService, path))
        {
            return (httpContext.ApiValidationProblem(detail: localizer["Cannot create folder inside a system folder"]), null);
        }

        var newPath = mediaFileStore.Combine(path, name);

        if (!await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMedia)
            || !await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMediaFolder, (object)newPath))
        {
            return (httpContext.ApiForbidProblem(), null);
        }

        var mediaFolder = await mediaFileStore.GetDirectoryInfoAsync(newPath);
        if (mediaFolder != null)
        {
            return (httpContext.ApiValidationProblem(detail: localizer["Cannot create folder because a folder already exists with the same name"]), null);
        }

        var existingFile = await mediaFileStore.GetFileInfoAsync(newPath);
        if (existingFile != null)
        {
            return (httpContext.ApiValidationProblem(detail: localizer["Cannot create folder because a file already exists with the same name"]), null);
        }

        await mediaFileStore.TryCreateDirectoryAsync(newPath);
        directoryTreeCache.Invalidate();

        mediaFolder = await mediaFileStore.GetDirectoryInfoAsync(newPath);

        return (null, mediaFolder);
    }
}
