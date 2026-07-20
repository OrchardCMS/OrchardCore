using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.FileStorage;
using OrchardCore.Media.Services;
using OrchardCore.Media.ViewModels;

namespace OrchardCore.Media.Endpoints.Api;

public static class UploadMediaEndpoint
{
    public static IEndpointRouteBuilder AddUploadMediaEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("api/media/Upload", HandleAsync)
            .WithName("ApiUploadMedia")
            .WithTags("MediaApi")
            .DisableAntiforgery()
            .AddEndpointFilter<MediaApiAntiforgeryEndpointFilter>()
            .Produces<UploadFilesResultDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        return builder;
    }

    [Authorize(Policy = MediaApiConstants.AuthorizationPolicyName)]
    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        IAuthorizationService authorizationService,
        IMediaFileStore mediaFileStore,
        IMediaNameNormalizerService mediaNameNormalizerService,
        IContentTypeProvider contentTypeProvider,
        IFileVersionProvider fileVersionProvider,
        IChunkFileUploadService chunkFileUploadService,
        FileCreationService fileCreationService,
        IServiceProvider serviceProvider,
        IOptions<MediaOptions> options,
        ILogger<MediaApiEndpoints> logger,
        IStringLocalizer<MediaApiEndpoints> localizer,
        string path,
        string extensions)
    {
        if (string.IsNullOrEmpty(path))
        {
            path = string.Empty;
        }

        if (!await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMedia)
            || !await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMediaFolder, (object)path))
        {
            return httpContext.ApiForbidProblem();
        }

        var mediaOptions = options.Value;

        // Replicate the [MediaSizeLimit] filter: cap the multipart body / request size using the
        // configured MaxFileSize before the form is read by the chunk upload service.
        ApplyMediaSizeLimit(httpContext, mediaOptions.MaxFileSize);

        var allowedExtensions = MediaEndpointHelpers.GetRequestedExtensions(mediaOptions, extensions, true);

        var actionResult = await chunkFileUploadService.ProcessRequestAsync(
            httpContext.Request,

            // We need this empty object because the frontend expects a JSON object in the response.
            (_, _, _) => Task.FromResult<IActionResult>(new OkObjectResult(new { })),
            async (files) =>
            {
                var result = new List<UploadFileResultDto>();

                // Loop through each file in the request.
                foreach (var file in files)
                {
                    var extension = Path.GetExtension(file.FileName);

                    if (!allowedExtensions.Contains(extension))
                    {
                        result.Add(new UploadFileResultDto
                        {
                            Name = file.FileName,
                            Size = file.Length,
                            Folder = path,
                            Error = localizer["This file extension is not allowed: {0}", extension].ToString(),
                        });

                        if (logger.IsEnabled(LogLevel.Information))
                        {
                            logger.LogInformation("File extension not allowed: '{File}'", file.FileName);
                        }

                        continue;
                    }

                    var fileName = mediaNameNormalizerService.NormalizeFileName(file.FileName);

                    Stream stream = null;
                    try
                    {
                        var mediaFilePath = mediaFileStore.Combine(path, fileName);

                        if (await mediaFileStore.GetFileInfoAsync(mediaFilePath) != null)
                        {
                            result.Add(new UploadFileResultDto
                            {
                                Name = fileName,
                                Size = file.Length,
                                Folder = path,
                                Error = localizer["A file with this name already exists in the current folder."].ToString(),
                            });

                            continue;
                        }

                        stream = file.OpenReadStream();
                        mediaFilePath = await mediaFileStore.CreateFileFromStreamAsync(
                            fileCreationService,
                            mediaFilePath,
                            stream,
                            length: file.Length,
                            contentType: file.ContentType,
                            cancellationToken: httpContext.RequestAborted);

                        var mediaFile = await mediaFileStore.GetFileInfoAsync(mediaFilePath);

                        await MediaEndpointHelpers.PreCacheRemoteMediaAsync(mediaFile, serviceProvider, mediaFileStore, httpContext);

                        result.Add(new UploadFileResultDto(MediaEndpointHelpers.CreateFileResult(mediaFile, httpContext, contentTypeProvider, fileVersionProvider, mediaFileStore)));
                    }
                    catch (ExistsFileStoreException ex)
                    {
                        logger.LogWarning(ex, "An error occurred while uploading a media");

                        result.Add(new UploadFileResultDto
                        {
                            Name = fileName,
                            Size = file.Length,
                            Folder = path,
                            Error = ex.Message,
                        });
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "An error occurred while uploading a media");

                        result.Add(new UploadFileResultDto
                        {
                            Name = fileName,
                            Size = file.Length,
                            Folder = path,
                            Error = ex.Message,
                        });
                    }
                    finally
                    {
                        stream?.Dispose();
                    }
                }

                return new OkObjectResult(new UploadFilesResultDto { Files = result });
            });

        // The chunk upload service is expressed in MVC IActionResult terms; bridge its result to
        // the minimal-API IResult pipeline by executing it against the current HttpContext.
        return new ActionResultResult(actionResult);
    }

    // Mirrors OrchardCore.Media.Services.MediaSizeLimitAttribute's InternalMediaSizeFilter, applied
    // inline because minimal-API endpoints do not run MVC authorization filters.
    private static void ApplyMediaSizeLimit(HttpContext httpContext, long maxFileSize)
    {
        var features = httpContext.Features;
        var formFeature = features.Get<IFormFeature>();

        if (formFeature == null || formFeature.Form == null)
        {
            // Request form has not been read yet, so set the limits.
            features.Set<IFormFeature>(new FormFeature(httpContext.Request, new FormOptions
            {
                MultipartBodyLengthLimit = maxFileSize,
            }));
        }

        // Will only be available when running OutOfProcess with Kestrel.
        var maxRequestBodySizeFeature = features.Get<IHttpMaxRequestBodySizeFeature>();
        if (maxRequestBodySizeFeature != null && !maxRequestBodySizeFeature.IsReadOnly)
        {
            maxRequestBodySizeFeature.MaxRequestBodySize = maxFileSize;
        }
    }

    // Adapts an MVC IActionResult to a minimal-API IResult so an endpoint can return the result
    // produced by IChunkFileUploadService.ProcessRequestAsync.
    private sealed class ActionResultResult : IResult
    {
        private readonly IActionResult _actionResult;

        public ActionResultResult(IActionResult actionResult)
            => _actionResult = actionResult;

        public Task ExecuteAsync(HttpContext httpContext)
        {
            var actionContext = new ActionContext(httpContext, new RouteData(httpContext.Request.RouteValues), new ActionDescriptor());

            return _actionResult.ExecuteResultAsync(actionContext);
        }
    }
}
