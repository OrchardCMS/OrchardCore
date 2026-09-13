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
using OrchardCore.Media.ViewModels;
using OrchardCore.RemoteManagement;

namespace OrchardCore.Media.Endpoints.Api;

public static class UploadMediaEndpoint
{
    public static IEndpointRouteBuilder AddUploadMediaEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapLegacyPost("api/media/Upload", HandleLegacyAsync)
            .AddEndpointFilter<MediaApiAntiforgeryEndpointFilter>();

        builder.MapManagementPut("api/media/files/content", HandleStreamAsync)
            .WithName("ApiUploadMedia")
            .WithSummary("Uploads a media file from a binary stream.")
            .WithDescription("Creates a media file from the request body stream. The legacy multipart upload endpoint remains available for the admin UI but is hidden from API discovery.")
            .WithCliCommand(new CliOperationMetadata(["media", "files"], "upload")
            {
                Capability = MediaApiEndpointConventions.CapabilityName,
                InputMode = CliInputMode.Stream,
                Arguments =
                {
                    new CliArgumentMetadata("fileName", 0),
                },
                TableColumns =
                {
                    new CliTableColumnMetadata("name", "Name"),
                    new CliTableColumnMetadata("filePath", "Path"),
                    new CliTableColumnMetadata("url", "URL"),
                    new CliTableColumnMetadata("size", "Size"),
                },
            })
            .Accepts<Stream>("application/octet-stream")
            .Produces<FileStoreEntryDto>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status413PayloadTooLarge);

        return builder;
    }

    private static async Task<IResult> HandleLegacyAsync(
        HttpContext httpContext,
        [FromServices] IAuthorizationService authorizationService,
        [FromServices] IMediaFileStore mediaFileStore,
        [FromServices] IMediaNameNormalizerService mediaNameNormalizerService,
        [FromServices] IContentTypeProvider contentTypeProvider,
        [FromServices] IFileVersionProvider fileVersionProvider,
        [FromServices] IChunkFileUploadService chunkFileUploadService,
        [FromServices] FileCreationService fileCreationService,
        [FromServices] IServiceProvider serviceProvider,
        [FromServices] IOptions<MediaOptions> options,
        [FromServices] ILogger<MediaApiEndpoints> logger,
        [FromServices] IStringLocalizer<MediaApiEndpoints> localizer,
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
        var canUploadRestrictedMedia = await authorizationService.AuthorizeAsync(
            httpContext.User,
            MediaPermissions.UploadRestrictedMedia);

        // Replicate the [MediaSizeLimit] filter: cap the multipart body / request size using the
        // configured MaxFileSize before the form is read by the chunk upload service.
        ApplyMediaSizeLimit(httpContext, mediaOptions.MaxFileSize);

        var requestedExtensions = MediaEndpointHelpers.GetRequestedExtensions(extensions);

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
                    if (!MediaEndpointHelpers.IsBaseName(file.FileName))
                    {
                        result.Add(new UploadFileResultDto
                        {
                            Name = file.FileName,
                            Size = file.Length,
                            Folder = path,
                            Error = localizer["The file name must not contain a directory path."].ToString(),
                        });
                        continue;
                    }

                    var normalizedFileName = mediaNameNormalizerService.NormalizeFileName(file.FileName);
                    if (!MediaEndpointHelpers.IsBaseName(normalizedFileName))
                    {
                        result.Add(new UploadFileResultDto
                        {
                            Name = normalizedFileName,
                            Size = file.Length,
                            Folder = path,
                            Error = localizer["The normalized file name must not contain a directory path."].ToString(),
                        });
                        continue;
                    }

                    var fileName = MediaEndpointHelpers.GetFileName(mediaFileStore, normalizedFileName);
                    var extension = Path.GetExtension(fileName);

                    if (!mediaOptions.IsFileExtensionAllowed(extension, canUploadRestrictedMedia)
                        || (requestedExtensions.Count > 0 && !requestedExtensions.Contains(extension)))
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

                    Stream stream = null;
                    try
                    {
                        var mediaFilePath = mediaFileStore.Combine(path, fileName);
                        if (!await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMediaFolder, (object)mediaFilePath))
                        {
                            result.Add(new UploadFileResultDto
                            {
                                Name = fileName,
                                Size = file.Length,
                                Folder = path,
                                Error = localizer["You do not have permission to manage this media path."].ToString(),
                            });
                            continue;
                        }

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

                        await MediaEndpointHelpers.PreCacheRemoteMediaAsync(
                            mediaFile,
                            mediaFileStore,
                            serviceProvider.GetService(typeof(IMediaFileStoreCache)) as IMediaFileStoreCache,
                            httpContext,
                            logger);

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

    private static async Task<IResult> HandleStreamAsync(
        HttpContext httpContext,
        [FromServices] IAuthorizationService authorizationService,
        [FromServices] IMediaFileStore mediaFileStore,
        [FromServices] IMediaNameNormalizerService mediaNameNormalizerService,
        [FromServices] IContentTypeProvider contentTypeProvider,
        [FromServices] IFileVersionProvider fileVersionProvider,
        [FromServices] FileCreationService fileCreationService,
        [FromServices] IServiceProvider serviceProvider,
        [FromServices] IOptions<MediaOptions> options,
        [FromServices] ILogger<MediaApiEndpoints> logger,
        [FromServices] IStringLocalizer<MediaApiEndpoints> localizer,
        [AsParameters] UploadMediaStreamRequest request)
    {
        var path = string.IsNullOrEmpty(request.Path) ? string.Empty : request.Path;

        if (!await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMedia)
            || !await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMediaFolder, (object)path))
        {
            return httpContext.ApiForbidProblem();
        }

        if (string.IsNullOrWhiteSpace(request.FileName))
        {
            return httpContext.ApiValidationProblem(detail: localizer["A file name is required."]);
        }

        if (!MediaEndpointHelpers.IsBaseName(request.FileName))
        {
            return httpContext.ApiValidationProblem(detail: localizer["The file name must not contain a directory path."]);
        }

        var mediaOptions = options.Value;
        ApplyRequestSizeLimit(httpContext, mediaOptions.MaxFileSize);
        if (httpContext.Request.ContentLength > mediaOptions.MaxFileSize)
        {
            return TypedResults.Problem(
                detail: localizer["The file exceeds the maximum allowed size of {0} bytes.", mediaOptions.MaxFileSize],
                statusCode: StatusCodes.Status413PayloadTooLarge);
        }

        var canUploadRestrictedMedia = await authorizationService.AuthorizeAsync(
            httpContext.User,
            MediaPermissions.UploadRestrictedMedia);
        var extension = Path.GetExtension(request.FileName);

        if (!mediaOptions.IsFileExtensionAllowed(extension, canUploadRestrictedMedia))
        {
            return httpContext.ApiValidationProblem(detail: localizer["This file extension is not allowed: {0}", extension]);
        }

        var normalizedFileName = mediaNameNormalizerService.NormalizeFileName(request.FileName);
        if (!MediaEndpointHelpers.IsBaseName(normalizedFileName))
        {
            return httpContext.ApiValidationProblem(detail: localizer["The normalized file name must not contain a directory path."]);
        }

        var fileName = MediaEndpointHelpers.GetFileName(mediaFileStore, normalizedFileName);
        var mediaFilePath = mediaFileStore.Combine(path, fileName);
        if (!await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMediaFolder, (object)mediaFilePath))
        {
            return httpContext.ApiForbidProblem();
        }

        if (await mediaFileStore.GetFileInfoAsync(mediaFilePath) != null)
        {
            return httpContext.ApiValidationProblem(detail: localizer["A file with this name already exists in the current folder."]);
        }

        try
        {
            using var limitedBody = new SizeLimitedReadStream(
                httpContext.Request.Body,
                mediaOptions.MaxFileSize,
                httpContext.Request.ContentLength);
            Stream uploadStream = limitedBody;
            MemoryStream bufferedBody = null;
            if (!httpContext.Request.ContentLength.HasValue)
            {
                bufferedBody = new MemoryStream();
                await limitedBody.CopyToAsync(bufferedBody, httpContext.RequestAborted);
                bufferedBody.Position = 0;
                uploadStream = bufferedBody;
            }

            using (bufferedBody)
            {
                var createdPath = await mediaFileStore.CreateFileFromStreamAsync(
                    fileCreationService,
                    mediaFilePath,
                    uploadStream,
                    length: uploadStream.Length,
                    contentType: httpContext.Request.ContentType,
                    cancellationToken: httpContext.RequestAborted);

                var mediaFile = await mediaFileStore.GetFileInfoAsync(createdPath);

                await MediaEndpointHelpers.PreCacheRemoteMediaAsync(
                    mediaFile,
                    mediaFileStore,
                    serviceProvider.GetService(typeof(IMediaFileStoreCache)) as IMediaFileStoreCache,
                    httpContext,
                    logger);

                return TypedResults.Ok(MediaEndpointHelpers.CreateFileResult(mediaFile, httpContext, contentTypeProvider, fileVersionProvider, mediaFileStore));
            }
        }
        catch (PayloadTooLargeException)
        {
            await mediaFileStore.TryDeleteFileAsync(mediaFilePath);
            return TypedResults.Problem(
                detail: localizer["The file exceeds the maximum allowed size of {0} bytes.", mediaOptions.MaxFileSize],
                statusCode: StatusCodes.Status413PayloadTooLarge);
        }
        catch (ExistsFileStoreException ex)
        {
            logger.LogWarning(ex, "An error occurred while streaming a media upload");

            return TypedResults.Problem(detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
        catch (FileStoreException ex)
        {
            logger.LogWarning(ex, "An error occurred while streaming a media upload");

            return TypedResults.Problem(detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while streaming a media upload");

            return TypedResults.Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
        }
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

    private static void ApplyRequestSizeLimit(HttpContext httpContext, long maxFileSize)
    {
        var maxRequestBodySizeFeature = httpContext.Features.Get<IHttpMaxRequestBodySizeFeature>();
        if (maxRequestBodySizeFeature != null && !maxRequestBodySizeFeature.IsReadOnly)
        {
            maxRequestBodySizeFeature.MaxRequestBodySize = maxFileSize;
        }
    }

    private sealed class PayloadTooLargeException : IOException;

    private sealed class SizeLimitedReadStream : Stream
    {
        private readonly Stream _inner;
        private readonly long _maximumLength;
        private readonly long? _length;
        private long _totalRead;

        public SizeLimitedReadStream(Stream inner, long maximumLength, long? length)
        {
            _inner = inner;
            _maximumLength = maximumLength;
            _length = length;
        }

        public override bool CanRead => _inner.CanRead;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => _length ?? throw new NotSupportedException();
        public override long Position
        {
            get => _totalRead;
            set => throw new NotSupportedException();
        }

        public override void Flush() => throw new NotSupportedException();
        public override int Read(byte[] buffer, int offset, int count)
        {
            var read = _inner.Read(buffer, offset, GetReadCount(count));
            AddRead(read);
            return read;
        }

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            var read = await _inner.ReadAsync(buffer[..GetReadCount(buffer.Length)], cancellationToken);
            AddRead(read);
            return read;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            var read = await _inner.ReadAsync(buffer.AsMemory(offset, GetReadCount(count)), cancellationToken);
            AddRead(read);
            return read;
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            // The request owns the underlying body stream.
            base.Dispose(disposing);
        }

        private int GetReadCount(int requestedCount)
        {
            var remaining = _maximumLength - _totalRead;
            return checked((int)Math.Min(requestedCount, Math.Max(0, remaining) + 1));
        }

        private void AddRead(int read)
        {
            _totalRead += read;
            if (_totalRead > _maximumLength)
            {
                throw new PayloadTooLargeException();
            }
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
