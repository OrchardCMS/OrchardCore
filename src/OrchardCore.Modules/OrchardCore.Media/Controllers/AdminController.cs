using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.FileStorage;
using OrchardCore.Media.Services;
using OrchardCore.Media.ViewModels;

namespace OrchardCore.Media.Controllers;

[Admin("Media/{action}", "Media.{action}")]
public sealed class AdminController : Controller
{
    private static readonly char[] _invalidFolderNameCharacters = ['\\', '/'];
    private static readonly char[] _extensionSeparator = [' ', ','];

    private readonly IMediaFileStore _mediaFileStore;
    private readonly IMediaNameNormalizerService _mediaNameNormalizerService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IContentTypeProvider _contentTypeProvider;
    private readonly ILogger _logger;
    private readonly MediaOptions _mediaOptions;
    private readonly IUserAssetFolderNameProvider _userAssetFolderNameProvider;
    private readonly IChunkFileUploadService _chunkFileUploadService;
    private readonly IFileVersionProvider _fileVersionProvider;
    private readonly IServiceProvider _serviceProvider;
    private readonly AttachedMediaFieldFileService _attachedMediaFieldFileService;

    internal readonly IStringLocalizer S;

    public AdminController(
        IMediaFileStore mediaFileStore,
        IMediaNameNormalizerService mediaNameNormalizerService,
        IAuthorizationService authorizationService,
        IContentTypeProvider contentTypeProvider,
        IOptions<MediaOptions> options,
        ILogger<AdminController> logger,
        IStringLocalizer<AdminController> stringLocalizer,
        IUserAssetFolderNameProvider userAssetFolderNameProvider,
        IChunkFileUploadService chunkFileUploadService,
        IFileVersionProvider fileVersionProvider,
        IServiceProvider serviceProvider,
        AttachedMediaFieldFileService attachedMediaFieldFileService)
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
        _fileVersionProvider = fileVersionProvider;
        _serviceProvider = serviceProvider;
        _attachedMediaFieldFileService = attachedMediaFieldFileService;
    }

    [Admin("Media", "Media.Index")]
    public async Task<IActionResult> Index()
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageMedia))
        {
            return Forbid();
        }

        return View();
    }

    public async Task<ActionResult<IEnumerable<MediaFolderViewModel>>> GetFolders(string path)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageMedia))
        {
            return Forbid();
        }

        if (string.IsNullOrEmpty(path))
        {
            path = string.Empty;
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

        return Ok(await allowed.Select(folder =>
        {
            var isSpecial = IsSpecialFolder(folder.Path);
            return new MediaFolderViewModel()
            {
                Name = folder.Name,
                Path = folder.Path,
                DirectoryPath = folder.DirectoryPath,
                IsDirectory = true,
                LastModifiedUtc = folder.LastModifiedUtc,
                Length = folder.Length,
                CanCreateFolder = !isSpecial,
                CanDeleteFolder = !isSpecial
            };
        }).ToListAsync());
    }

    public async Task<ActionResult<IEnumerable<object>>> GetMediaItems(string path, string extensions)
    {
        if (string.IsNullOrEmpty(path))
        {
            path = string.Empty;
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
            .WhereAwait(async e =>
                !e.IsDirectory
                && (allowedExtensions.Count == 0 || allowedExtensions.Contains(Path.GetExtension(e.Path)))
                && await _authorizationService.AuthorizeAsync(User, Permissions.ManageMediaFolder, (object)e.Path))
            .Select(e => CreateFileResult(e));

        return Ok(await allowed.ToListAsync());
    }

    public async Task<ActionResult<object>> GetMediaItem(string path)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageMedia)
            || (HttpContext.IsSecureMediaEnabled() && !await _authorizationService.AuthorizeAsync(User, SecureMediaPermissions.ViewMedia, (object)(path ?? string.Empty))))
        {
            return Forbid();
        }

        if (string.IsNullOrEmpty(path))
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
    public async Task<IActionResult> Upload(string path, string extensions)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageMedia)
            || (HttpContext.IsSecureMediaEnabled() && !await _authorizationService.AuthorizeAsync(User, SecureMediaPermissions.ViewMedia, (object)(path ?? string.Empty))))
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
                if (string.IsNullOrEmpty(path))
                {
                    path = string.Empty;
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

                        // The .NET AWS SDK, and only that from the built-in ones (but others maybe too), disposes
                        // the stream. There's no better way to check for that than handling the exception. An
                        // alternative would be to re-read the file for every other storage provider as well but
                        // that would be wasteful.
                        try
                        {
                            stream.Position = 0;
                        }
                        catch (ObjectDisposedException)
                        {
                            stream = null;
                        }

                        await PreCacheRemoteMedia(mediaFile, stream);

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
    public async Task<IActionResult> DeleteFolder(string path)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageMedia)
            || !await _authorizationService.AuthorizeAsync(User, Permissions.ManageMediaFolder, (object)path))
        {
            return Forbid();
        }

        if (string.IsNullOrEmpty(path))
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
    public async Task<IActionResult> DeleteMedia(string path)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageMedia)
            || !await _authorizationService.AuthorizeAsync(User, Permissions.ManageMediaFolder, (object)path))
        {
            return Forbid();
        }

        if (string.IsNullOrEmpty(path))
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
    public async Task<IActionResult> MoveMedia(string oldPath, string newPath)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageMedia)
            || !await _authorizationService.AuthorizeAsync(User, Permissions.ManageMediaFolder, (object)oldPath)
            || !await _authorizationService.AuthorizeAsync(User, Permissions.ManageMediaFolder, (object)newPath))
        {
            return Forbid();
        }

        if (string.IsNullOrEmpty(oldPath) || string.IsNullOrEmpty(newPath))
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

        var newFileInfo = await _mediaFileStore.GetFileInfoAsync(newPath);
        await PreCacheRemoteMedia(newFileInfo);

        return Ok(new { newUrl = GetCacheBustingMediaPublicUrl(newPath) });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteMediaList(string[] paths)
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
    public async Task<IActionResult> MoveMediaList(string[] mediaNames, string sourceFolder, string targetFolder)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageMedia)
            || !await _authorizationService.AuthorizeAsync(User, Permissions.ManageMediaFolder, (object)sourceFolder)
            || !await _authorizationService.AuthorizeAsync(User, Permissions.ManageMediaFolder, (object)targetFolder))
        {
            return Forbid();
        }

        if ((mediaNames == null) || (mediaNames.Length < 1)
            || string.IsNullOrEmpty(sourceFolder)
            || string.IsNullOrEmpty(targetFolder))
        {
            return NotFound();
        }

        sourceFolder = sourceFolder == "root" ? string.Empty : sourceFolder;
        targetFolder = targetFolder == "root" ? string.Empty : targetFolder;

        var filesOnError = new List<string>();

        foreach (var name in mediaNames)
        {
            var sourcePath = _mediaFileStore.Combine(sourceFolder, name);
            var targetPath = _mediaFileStore.Combine(targetFolder, name);
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
            return BadRequest(S["Error when moving files. Maybe they already exist on the target folder? Files on error: {0}", string.Join(",", filesOnError)].ToString());
        }

        return Ok();
    }

    [HttpPost]
    public async Task<ActionResult<IFileStoreEntry>> CreateFolder(
        string path, string name,
        [FromServices] IAuthorizationService authorizationService)
    {
        if (string.IsNullOrEmpty(path))
        {
            path = string.Empty;
        }

        name = _mediaNameNormalizerService.NormalizeFolderName(name);

        if (_invalidFolderNameCharacters.Any(invalidChar => name.Contains(invalidChar)))
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

    public object CreateFileResult(IFileStoreEntry mediaFile)
    {
        _contentTypeProvider.TryGetContentType(mediaFile.Name, out var contentType);

        return new
        {
            name = mediaFile.Name,
            size = mediaFile.Length,
            lastModify = mediaFile.LastModifiedUtc.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds,
            folder = mediaFile.DirectoryPath,
            url = GetCacheBustingMediaPublicUrl(mediaFile.Path),
            mediaPath = mediaFile.Path,
            mime = contentType ?? "application/octet-stream",
            mediaText = string.Empty,
            anchor = new { x = 0.5f, y = 0.5f },
            attachedFileName = string.Empty
        };
    }

    public async Task<IActionResult> MediaApplication(MediaApplicationViewModel model)
    {
        // Check if the user has access to new folders. If not, we hide the "create folder" button from the root folder.
        model.AllowNewRootFolders = !HttpContext.IsSecureMediaEnabled() || await _authorizationService.AuthorizeAsync(User, SecureMediaPermissions.ViewMedia, (object)"_non-existent-path-87FD1922-8F88-4A33-9766-DA03E6E6F7BA");

        return View(model);
    }

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
        if (!string.IsNullOrWhiteSpace(exts))
        {
            var extensions = exts.Split(_extensionSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

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

        return [];
    }

    private string GetCacheBustingMediaPublicUrl(string path) =>
        _fileVersionProvider.AddFileVersionToPath(HttpContext.Request.PathBase, _mediaFileStore.MapPathToPublicUrl(path));

    // If a remote storage is used, then we need to preemptively cache the newly uploaded or renamed file. Without
    // this, the Media Library page will try to load the thumbnail without a cache busting parameter, since
    // ShellFileVersionProvider won't find it in the local cache.
    // This is not required for files moved across folders, because the folder will be reopened anyway.
    private async Task PreCacheRemoteMedia(IFileStoreEntry mediaFile, Stream stream = null)
    {
        var mediaFileStoreCache = _serviceProvider.GetService<IMediaFileStoreCache>();
        if (mediaFileStoreCache == null)
        {
            return;
        }

        Stream localStream = null;

        if (stream == null)
        {
            stream = localStream = await _mediaFileStore.GetFileStreamAsync(mediaFile);
        }

        try
        {
            await mediaFileStoreCache.SetCacheAsync(stream, mediaFile, HttpContext.RequestAborted);
        }
        finally
        {
            localStream?.Dispose();
        }
    }

    private bool IsSpecialFolder(string path)
       => string.Equals(path, _mediaOptions.AssetsUsersFolder, StringComparison.OrdinalIgnoreCase) || string.Equals(path, _attachedMediaFieldFileService.MediaFieldsFolder, StringComparison.OrdinalIgnoreCase);
}
