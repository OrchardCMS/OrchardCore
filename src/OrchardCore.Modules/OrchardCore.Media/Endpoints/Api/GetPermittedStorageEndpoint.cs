using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Media.Core.Helpers;
using OrchardCore.Media.ViewModels;
using OrchardCore.RemoteManagement;
using OrchardCore.Settings;

namespace OrchardCore.Media.Endpoints.Api;

public static class GetPermittedStorageEndpoint
{
    public static IEndpointRouteBuilder AddGetPermittedStorageEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapLegacyGet("api/media/GetPermittedStorage", HandleLegacyAsync);

        builder.MapManagementGet("api/media/constraints", HandleAsync)
            .WithName("ApiGetPermittedStorage")
            .WithSummary("Shows the media storage constraints.")
            .WithDescription("Returns the configured media storage limits, authentication mode, and file extensions the current user may upload, including restricted extensions when the user has UploadRestrictedMedia permission.")
            .WithCliCommand(new CliOperationMetadata(["media", "constraints"], "show")
            {
                Capability = MediaApiEndpointConventions.CapabilityName,
                TableColumns =
                {
                    new CliTableColumnMetadata("text", "Permitted storage"),
                    new CliTableColumnMetadata("maxFileSize", "Max file size"),
                    new CliTableColumnMetadata("authenticationScheme", "Authentication"),
                },
            })
            .Produces<MediaConstraintsDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        return builder;
    }

    private static async Task<IResult> HandleLegacyAsync(
        HttpContext httpContext,
        [FromServices] IAuthorizationService authorizationService,
        [FromServices] IMediaFileStore mediaFileStore,
        [FromServices] IStringLocalizer<MediaApiEndpoints> localizer,
        [FromServices] FileSizeHelper fileSizeHelper)
    {
        var (result, bytes, text) = await GetStorageConstraintsAsync(httpContext, authorizationService, mediaFileStore, localizer, fileSizeHelper);

        return result ?? TypedResults.Ok(new PermittedStorageDto
        {
            Bytes = bytes,
            Text = text,
        });
    }

    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        [FromServices] IAuthorizationService authorizationService,
        [FromServices] IMediaFileStore mediaFileStore,
        [FromServices] IStringLocalizer<MediaApiEndpoints> localizer,
        [FromServices] ISiteService siteService,
        [FromServices] IOptions<MediaOptions> options,
        [FromServices] FileSizeHelper fileSizeHelper)
    {
        var (result, bytes, text) = await GetStorageConstraintsAsync(httpContext, authorizationService, mediaFileStore, localizer, fileSizeHelper);

        if (result is not null)
        {
            return result;
        }

        var mediaOptions = options.Value;
        var canUploadRestrictedMedia = await authorizationService.AuthorizeAsync(
            httpContext.User,
            MediaPermissions.UploadRestrictedMedia);

        return TypedResults.Ok(new MediaConstraintsDto
        {
            Bytes = bytes,
            Text = text,
            AllowedFileExtensions = mediaOptions.AllowedFileExtensions
                .Concat(canUploadRestrictedMedia ? mediaOptions.RestrictedFileExtensions : [])
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Order(StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            MaxFileSize = mediaOptions.MaxFileSize,
            MaxUploadChunkSize = mediaOptions.MaxUploadChunkSize,
            AuthenticationScheme = siteService.GetSettings<MediaApiSettings>().AuthenticationScheme.ToString(),
        });
    }

    private static async Task<(IResult Result, long? Bytes, string Text)> GetStorageConstraintsAsync(
        HttpContext httpContext,
        IAuthorizationService authorizationService,
        IMediaFileStore mediaFileStore,
        IStringLocalizer<MediaApiEndpoints> localizer,
        FileSizeHelper fileSizeHelper)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMedia)
            || !await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMediaFolder, (object)string.Empty))
        {
            return (httpContext.ApiForbidProblem(), null, null);
        }

        var bytes = await mediaFileStore.GetPermittedStorageAsync();
        var text = bytes == null ? localizer["Unspecified"] : fileSizeHelper.FormatSize(bytes.Value);

        return (null, bytes, text);
    }
}
