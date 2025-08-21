using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.FileStorage;
using OrchardCore.Media.Services;
using OrchardCore.Media.ViewModels;

namespace OrchardCore.Media.Controllers;

[ApiController]
[Route("api/media")]
[Authorize(AuthenticationSchemes = "Api"), IgnoreAntiforgeryToken, AllowAnonymous]
public class ApiController : Controller
{
    private static readonly char[] InvalidFolderNameCharacters = new char[] { '\\', '/' };
    private static readonly char[] ExtensionSeperator = new char[] { ' ', ',' };
    private static readonly HashSet<string> EmptySet = new();

    private readonly IMediaFileStore _mediaFileStore;
    private readonly IMediaNameNormalizerService _mediaNameNormalizerService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IContentTypeProvider _contentTypeProvider;
    private readonly ILogger _logger;
    protected readonly IStringLocalizer S;
    private readonly MediaOptions _mediaOptions;
    private readonly IUserAssetFolderNameProvider _userAssetFolderNameProvider;
    private readonly IChunkFileUploadService _chunkFileUploadService;

    public ApiController(
        IMediaFileStore mediaFileStore,
        IMediaNameNormalizerService mediaNameNormalizerService,
        IAuthorizationService authorizationService,
        IContentTypeProvider contentTypeProvider,
        IOptions<MediaOptions> options,
        ILogger<ApiController> logger,
        IStringLocalizer<ApiController> stringLocalizer,
        IUserAssetFolderNameProvider userAssetFolderNameProvider,
        IChunkFileUploadService chunkFileUploadService
        )
    {
        _mediaFileStore = mediaFileStore;
        _mediaNameNormalizerService = mediaNameNormalizerService;
        _authorizationService = authorizationService;
        _contentTypeProvider = contentTypeProvider;
        _mediaOptions = options.Value;
        _logger = logger;
        S = stringLocalizer;
        _userAssetFolderNameProvider = userAssetFolderNameProvider;
        _chunkFileUploadService = chunkFileUploadService;
    }

    [HttpGet]
    [Route("GetFolders")]
    public async Task<ActionResult<IEnumerable<IFileStoreEntry>>> GetFolders(string path)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageMedia))
        {
            return Forbid();
        }

        if (String.IsNullOrEmpty(path))
        {
            path = String.Empty;
        }

        if (await _mediaFileStore.GetDirectoryInfoAsync(path) == null)
        {
            return NotFound();
        }

        // create default folders if not exist
        if (await _authorizationService.AuthorizeAsync(User, Permissions.ManageOwnMedia)
            && await _mediaFileStore.GetDirectoryInfoAsync(_mediaFileStore.Combine(_mediaOptions.AssetsUsersFolder, _userAssetFolderNameProvider.GetUserAssetFolderName(User))) == null)
        {
            await _mediaFileStore.TryCreateDirectoryAsync(_mediaFileStore.Combine(_mediaOptions.AssetsUsersFolder, _userAssetFolderNameProvider.GetUserAssetFolderName(User)));
        }

        var allowed = _mediaFileStore.GetDirectoryContentAsync(path)
            .WhereAwait(async e => e.IsDirectory && await _authorizationService.AuthorizeAsync(User, Permissions.ManageMediaFolder, (object)e.Path));

        return Ok(await allowed.ToListAsync());
    }

    [HttpGet]
    [Route("GetMediaItems")]
    public async Task<ActionResult<IEnumerable<object>>> GetMediaItems(string path, string extensions)
    {
        if (String.IsNullOrEmpty(path))
        {
            path = String.Empty;
        }

        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageMedia)
            || !await _authorizationService.AuthorizeAsync(User, Permissions.ManageMediaFolder, (object)path))
        {
            return Forbid();
        }

        if (await _mediaFileStore.GetDirectoryInfoAsync(path) == null)
        {
            return NotFound();
        }

        var allowedExtensions = GetRequestedExtensions(extensions, false);

        var allowed = _mediaFileStore.GetDirectoryContentAsync(path)
            .WhereAwait(async e => !e.IsDirectory && (allowedExtensions.Count == 0 || allowedExtensions.Contains(Path.GetExtension(e.Path))) && await _authorizationService.AuthorizeAsync(User, Permissions.ManageMediaFolder, (object)e.Path))
            .Select(e => CreateFileResult(e));

        return Ok(await allowed.ToListAsync());
    }

    [HttpGet]
    [Route("GetMediaItem")]
    public async Task<ActionResult<object>> GetMediaItem(string path)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageMedia))
        {
            return Forbid();
        }

        if (String.IsNullOrEmpty(path))
        {
            return NotFound();
        }

        var fileEntry = await _mediaFileStore.GetFileInfoAsync(path);

        if (fileEntry == null)
        {
            return NotFound();
        }

        return CreateFileResult(fileEntry);
    }

    [HttpPost]
    [MediaSizeLimit]
    [Route("Upload")]
    public async Task<IActionResult> Upload(string path, string extensions)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageMedia))
        {
            return Forbid();
        }

        var allowedExtensions = GetRequestedExtensions(extensions, true);

        return await _chunkFileUploadService.ProcessRequestAsync(
            Request,

            // We need this empty object because the frontend expects a JSON object in the response.
            (_, _, _) => Task.FromResult<IActionResult>(Ok(new { })),
            async (files) =>
            {
                if (String.IsNullOrEmpty(path))
                {
                    path = String.Empty;
                }

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
                            error = S["This file extension is not allowed: {0}", extension].ToString()
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
                        stream = file.OpenReadStream();
                        mediaFilePath = await _mediaFileStore.CreateFileFromStreamAsync(mediaFilePath, stream);

                        var mediaFile = await _mediaFileStore.GetFileInfoAsync(mediaFilePath);

                        result.Add(CreateFileResult(mediaFile));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An error occurred while uploading a media");

                        result.Add(new
                        {
                            name = fileName,
                            size = file.Length,
                            folder = path,
                            error = ex.Message
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

    [HttpPost]
    [Route("DeleteFolder")]
    public async Task<IActionResult> DeleteFolder(string path)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageMedia)
            || !await _authorizationService.AuthorizeAsync(User, Permissions.ManageMediaFolder, (object)path))
        {
            return Forbid();
        }

        if (String.IsNullOrEmpty(path))
        {
            return StatusCode(StatusCodes.Status403Forbidden, S["Cannot delete root media folder"]);
        }

        var mediaFolder = await _mediaFileStore.GetDirectoryInfoAsync(path);
        if (mediaFolder != null && !mediaFolder.IsDirectory)
        {
            return StatusCode(StatusCodes.Status403Forbidden, S["Cannot delete path because it is not a directory"]);
        }

        if (await _mediaFileStore.TryDeleteDirectoryAsync(path) == false)
        {
            return NotFound();
        }

        return Ok();
    }

    [HttpPost]
    [Route("DeleteMedia")]
    public async Task<IActionResult> DeleteMedia(string path)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageMedia)
            || !await _authorizationService.AuthorizeAsync(User, Permissions.ManageMediaFolder, (object)path))
        {
            return Forbid();
        }

        if (String.IsNullOrEmpty(path))
        {
            return NotFound();
        }

        if (!await _mediaFileStore.TryDeleteFileAsync(path))
        {
            return NotFound();
        }

        return Ok();
    }

    [HttpPost]
    [Route("MoveMedia")]
    public async Task<IActionResult> MoveMedia(string oldPath, string newPath)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageMedia)
            || !await _authorizationService.AuthorizeAsync(User, Permissions.ManageMediaFolder, (object)oldPath))
        {
            return Forbid();
        }

        if (String.IsNullOrEmpty(oldPath) || String.IsNullOrEmpty(newPath))
        {
            return NotFound();
        }

        if (await _mediaFileStore.GetFileInfoAsync(oldPath) == null)
        {
            return NotFound();
        }

        var newExtension = Path.GetExtension(newPath);

        if (!_mediaOptions.AllowedFileExtensions.Contains(newExtension, StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(S["This file extension is not allowed: {0}", newExtension]);
        }

        if (await _mediaFileStore.GetFileInfoAsync(newPath) != null)
        {
            return BadRequest(S["Cannot move media because a file already exists with the same name"]);
        }

        await _mediaFileStore.MoveFileAsync(oldPath, newPath);

        return Ok();
    }

    [HttpPost]
    [Route("DeleteMediaList")]
    public async Task<IActionResult> DeleteMediaList([FromBody] List<String> paths)
    {
        if (paths == null)
        {
            return NotFound();
        }

        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageMedia))
        {
            return Forbid();
        }

        foreach (var path in paths)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageAttachedMediaFieldsFolder, (object)path))
            {
                return Forbid();
            }
        }

        foreach (var path in paths)
        {
            if (!await _mediaFileStore.TryDeleteFileAsync(path))
            {
                return NotFound();
            }
        }

        return Ok();
    }

    [HttpPost]
    [Route("MoveMediaList")]
    public async Task<IActionResult> MoveMediaList([FromBody] MoveMedias model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageMedia)
            || !await _authorizationService.AuthorizeAsync(User, Permissions.ManageMediaFolder, (object)model.sourceFolder)
            || !await _authorizationService.AuthorizeAsync(User, Permissions.ManageMediaFolder, (object)model.targetFolder))
        {
            return Forbid();
        }

        if ((model.mediaNames == null) || (model.mediaNames.Length < 1)
            || String.IsNullOrEmpty(model.sourceFolder)
            || String.IsNullOrEmpty(model.targetFolder))
        {
            return NotFound();
        }

        model.sourceFolder = model.sourceFolder == "root" ? String.Empty : model.sourceFolder;
        model.targetFolder = model.targetFolder == "root" ? String.Empty : model.targetFolder;

        var filesOnError = new List<string>();

        foreach (var name in model.mediaNames)
        {
            var sourcePath = _mediaFileStore.Combine(model.sourceFolder, name);
            var targetPath = _mediaFileStore.Combine(model.targetFolder, name);
            try
            {
                await _mediaFileStore.MoveFileAsync(sourcePath, targetPath);
            }
            catch (FileStoreException)
            {
                filesOnError.Add(sourcePath);
            }
        }

        if (filesOnError.Count > 0)
        {
            return BadRequest(S["Error when moving files. Maybe they already exist on the target folder? Files on error: {0}", String.Join(",", filesOnError)].ToString());
        }

        return Ok();
    }

    [HttpPost]
    [Route("CreateFolder")]
    public async Task<ActionResult<IFileStoreEntry>> CreateFolder(
        string path, string name,
        [FromServices] IAuthorizationService authorizationService)
    {
        if (String.IsNullOrEmpty(path))
        {
            path = String.Empty;
        }

        name = _mediaNameNormalizerService.NormalizeFolderName(name);

        if (InvalidFolderNameCharacters.Any(invalidChar => name.Contains(invalidChar)))
        {
            return BadRequest(S["Cannot create folder because the folder name contains invalid characters"]);
        }

        var newPath = _mediaFileStore.Combine(path, name);

        if (!await authorizationService.AuthorizeAsync(User, Permissions.ManageMedia)
            || !await authorizationService.AuthorizeAsync(User, Permissions.ManageMediaFolder, (object)newPath))
        {
            return Forbid();
        }

        var mediaFolder = await _mediaFileStore.GetDirectoryInfoAsync(newPath);
        if (mediaFolder != null)
        {
            return BadRequest(S["Cannot create folder because a folder already exists with the same name"]);
        }

        var existingFile = await _mediaFileStore.GetFileInfoAsync(newPath);
        if (existingFile != null)
        {
            return BadRequest(S["Cannot create folder because a file already exists with the same name"]);
        }

        await _mediaFileStore.TryCreateDirectoryAsync(newPath);

        mediaFolder = await _mediaFileStore.GetDirectoryInfoAsync(newPath);

        return new ObjectResult(mediaFolder);
    }

    [Route("CreateFileResult")]
    public object CreateFileResult(IFileStoreEntry mediaFile)
    {
        _contentTypeProvider.TryGetContentType(mediaFile.Name, out var contentType);

        return new
        {
            name = mediaFile.Name,
            size = mediaFile.Length,
            lastModify = mediaFile.LastModifiedUtc.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds,
            folder = mediaFile.DirectoryPath,
            url = _mediaFileStore.MapPathToPublicUrl(mediaFile.Path),
            mediaPath = mediaFile.Path,
            mime = contentType ?? "application/octet-stream",
            mediaText = String.Empty,
            anchor = new { x = 0.5f, y = 0.5f },
            attachedFileName = String.Empty
        };
    }

    [Route("Options")]
    public async Task<IActionResult> Options()
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ViewMediaOptions))
        {
            return Forbid();
        }

        return View(_mediaOptions);
    }

    private HashSet<string> GetRequestedExtensions(string exts, bool fallback)
    {
        if (!String.IsNullOrWhiteSpace(exts))
        {
            var extensions = exts.Split(ExtensionSeperator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var requestedExtensions = _mediaOptions.AllowedFileExtensions
                .Intersect(extensions)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (requestedExtensions.Count > 0)
            {
                return requestedExtensions;
            }
        }

        if (fallback)
        {
            return _mediaOptions.AllowedFileExtensions
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        return EmptySet;
    }
}

public class FileStoreEntry : IFileStoreEntry
{
    #region IFileStoreEntry
    [Required]
    public string Path { get; set; }

    [Required]
    public string Name { get; set; }

    public string DirectoryPath { get; set; }

    public long Length { get; set; }

    public DateTime LastModifiedUtc { get; set; }

    public bool IsDirectory { get; set; }

    #endregion

    public double LastModify { get; set; }
    public long Size { get; set; }
    public string Error { get; set; }
    public string Folder { get; set; }
    public string Url { get; set; }
    public string FilePath { get; set; }
    public string Mime { get; set; }
    public string MediaText { get; set; }
    public object Anchor { get; set; } // TODO make non anonymous
    public string AttachedFileName { get; set; }
}

public class MoveMedias
{
    public string[] mediaNames { get; set; }
    public string sourceFolder { get; set; }
    public string targetFolder { get; set; }
}
