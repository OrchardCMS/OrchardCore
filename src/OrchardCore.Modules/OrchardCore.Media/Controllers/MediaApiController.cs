using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.FileStorage;
using OrchardCore.Media.Endpoints.Api;
using OrchardCore.Media.Services;
using OrchardCore.Media.ViewModels;

namespace OrchardCore.Media.Controllers;

[ApiController]
[Route("api/media")]
[IgnoreAntiforgeryToken]
public class MediaApiController : Controller
{
    private readonly IMediaFileStore _mediaFileStore;
    private readonly IMediaNameNormalizerService _mediaNameNormalizerService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IContentTypeProvider _contentTypeProvider;
    private readonly ILogger _logger;
    protected readonly IStringLocalizer S;
    private readonly MediaOptions _mediaOptions;
    private readonly IChunkFileUploadService _chunkFileUploadService;
    private readonly IServiceProvider _serviceProvider;
    private readonly IFileVersionProvider _fileVersionProvider;
    private readonly FileCreationService _fileCreationService;

    public MediaApiController(
        IMediaFileStore mediaFileStore,
        IMediaNameNormalizerService mediaNameNormalizerService,
        IAuthorizationService authorizationService,
        IContentTypeProvider contentTypeProvider,
        IOptions<MediaOptions> options,
        ILogger<MediaApiController> logger,
        IStringLocalizer<MediaApiController> stringLocalizer,
        IChunkFileUploadService chunkFileUploadService,
        IServiceProvider serviceProvider,
        IFileVersionProvider fileVersionProvider,
        FileCreationService fileCreationService
        )
    {
        _mediaFileStore = mediaFileStore;
        _mediaNameNormalizerService = mediaNameNormalizerService;
        _authorizationService = authorizationService;
        _contentTypeProvider = contentTypeProvider;
        _mediaOptions = options.Value;
        _logger = logger;
        S = stringLocalizer;
        _chunkFileUploadService = chunkFileUploadService;
        _serviceProvider = serviceProvider;
        _fileVersionProvider = fileVersionProvider;
        _fileCreationService = fileCreationService;
    }

    [Authorize(AuthenticationSchemes = "Api")]
    [HttpPost]
    [MediaSizeLimit]
    [Route("Upload")]
    [EndpointName("ApiUploadMedia")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Upload(string path, string extensions)
    {
        if (String.IsNullOrEmpty(path))
        {
            path = String.Empty;
        }

        if (!await _authorizationService.AuthorizeAsync(User, MediaPermissions.ManageMedia)
            || !await _authorizationService.AuthorizeAsync(User, MediaPermissions.ManageMediaFolder, (object)path))
        {
            return this.ApiForbidProblem();
        }

        var allowedExtensions = MediaEndpointHelpers.GetRequestedExtensions(_mediaOptions, extensions, true);

        return await _chunkFileUploadService.ProcessRequestAsync(
            Request,

            // We need this empty object because the frontend expects a JSON object in the response.
            (_, _, _) => Task.FromResult<IActionResult>(Ok(new { })),
            async (files) =>
            {
                var result = new List<object>();

                // Loop through each file in the request.
                foreach (var file in files)
                {
                    var extension = Path.GetExtension(file.FileName);

                    if (!allowedExtensions.Contains(extension))
                    {
                        result.Add(new
                        {
                            name = file.FileName,
                            size = file.Length,
                            folder = path,
                            error = S["This file extension is not allowed: {0}", extension].ToString(),
                        });

                        if (_logger.IsEnabled(LogLevel.Information))
                        {
                            _logger.LogInformation("File extension not allowed: '{File}'", file.FileName);
                        }

                        continue;
                    }

                    var fileName = _mediaNameNormalizerService.NormalizeFileName(file.FileName);

                    Stream stream = null;
                    try
                    {
                        var mediaFilePath = _mediaFileStore.Combine(path, fileName);

                        if (await _mediaFileStore.GetFileInfoAsync(mediaFilePath) != null)
                        {
                            result.Add(new
                            {
                                name = fileName,
                                size = file.Length,
                                folder = path,
                                error = S["A file with this name already exists in the current folder."].ToString(),
                            });

                            continue;
                        }

                        stream = file.OpenReadStream();
                        mediaFilePath = await _mediaFileStore.CreateFileFromStreamAsync(
                            _fileCreationService,
                            mediaFilePath,
                            stream,
                            length: file.Length,
                            contentType: file.ContentType,
                            cancellationToken: HttpContext.RequestAborted);

                        var mediaFile = await _mediaFileStore.GetFileInfoAsync(mediaFilePath);

                        await MediaEndpointHelpers.PreCacheRemoteMediaAsync(mediaFile, _serviceProvider, _mediaFileStore, HttpContext);

                        result.Add(MediaEndpointHelpers.CreateFileResult(mediaFile, HttpContext, _contentTypeProvider, _fileVersionProvider, _mediaFileStore));
                    }
                    catch (ExistsFileStoreException ex)
                    {
                        _logger.LogWarning(ex, "An error occurred while uploading a media");

                        result.Add(new
                        {
                            name = fileName,
                            size = file.Length,
                            folder = path,
                            error = ex.Message,
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An error occurred while uploading a media");

                        result.Add(new
                        {
                            name = fileName,
                            size = file.Length,
                            folder = path,
                            error = ex.Message,
                        });
                    }
                    finally
                    {
                        stream?.Dispose();
                    }
                }

                return Ok(new { files = result.ToArray() });
            });
    }

    [Authorize]
    [Route("Options")]
    public async Task<IActionResult> Options()
    {
        if (!await _authorizationService.AuthorizeAsync(User, MediaPermissions.ViewMediaOptions))
        {
            return Forbid();
        }

        return View(_mediaOptions);
    }

    [Authorize(AuthenticationSchemes = "Api")]
    [HttpGet]
    [Route("TusFileInfo/{uploadId}")]
    [EndpointName("ApiGetTusFileInfo")]
    [ProducesResponseType(typeof(FileStoreEntryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FileStoreEntryDto>> GetTusFileInfo(
        string uploadId,
        [FromServices] DistributedTusUploadMetadataStore tusMetadataStore)
    {
        if (!await _authorizationService.AuthorizeAsync(User, MediaPermissions.ManageMedia))
        {
            return this.ApiForbidProblem();
        }

        var entry = await tusMetadataStore.GetAsync(uploadId);

        if (entry == null || string.IsNullOrEmpty(entry.MediaFilePath))
        {
            return this.ApiNotFoundProblem();
        }

        var fileInfo = await _mediaFileStore.GetFileInfoAsync(entry.MediaFilePath);

        if (fileInfo == null)
        {
            return this.ApiNotFoundProblem();
        }

        // Remove the entry now that the client has retrieved it.
        await tusMetadataStore.RemoveAsync(uploadId);

        return Ok(MediaEndpointHelpers.CreateFileResult(fileInfo, HttpContext, _contentTypeProvider, _fileVersionProvider, _mediaFileStore));
    }
}
