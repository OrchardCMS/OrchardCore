using System;
using System.Collections.Generic;
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

namespace OrchardCore.Media.Controllers
{
    public class AdminController : Controller
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

        public AdminController(
            IMediaFileStore mediaFileStore,
            IMediaNameNormalizerService mediaNameNormalizerService,
            IAuthorizationService authorizationService,
            IContentTypeProvider contentTypeProvider,
            IOptions<MediaOptions> options,
            ILogger<AdminController> logger,
            IStringLocalizer<AdminController> stringLocalizer,
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

        public async Task<IActionResult> Index()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageMedia))
            {
                return Forbid();
            }

            return View();
        }

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
                || String.IsNullOrEmpty(sourceFolder)
                || String.IsNullOrEmpty(targetFolder))
            {
                return NotFound();
            }

            sourceFolder = sourceFolder == "root" ? String.Empty : sourceFolder;
            targetFolder = targetFolder == "root" ? String.Empty : targetFolder;

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
                return BadRequest(S["Error when moving files. Maybe they already exist on the target folder? Files on error: {0}", String.Join(",", filesOnError)].ToString());
            }

            return Ok();
        }

        [HttpPost]
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

        public IActionResult MediaApplication(MediaApplicationViewModel model)
        {
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
}
