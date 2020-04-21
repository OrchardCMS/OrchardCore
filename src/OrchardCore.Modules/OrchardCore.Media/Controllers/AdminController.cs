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

namespace OrchardCore.Media.Controllers
{
    public class AdminController : Controller
    {
        private readonly HashSet<string> _allowedFileExtensions;
        private readonly IMediaFileStore _mediaFileStore;
        private readonly IAuthorizationService _authorizationService;
        private readonly IContentTypeProvider _contentTypeProvider;
        private readonly ILogger _logger;
        private readonly IStringLocalizer S;

        public AdminController(
            IMediaFileStore mediaFileStore,
            IAuthorizationService authorizationService,
            IContentTypeProvider contentTypeProvider,
            IOptions<MediaOptions> options,
            ILogger<AdminController> logger,
            IStringLocalizer<AdminController> stringLocalizer
            )
        {
            _mediaFileStore = mediaFileStore;
            _authorizationService = authorizationService;
            _contentTypeProvider = contentTypeProvider;
            _allowedFileExtensions = options.Value.AllowedFileExtensions;
            _logger = logger;
            S = stringLocalizer;
        }

        public async Task<IActionResult> Index()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageOwnMedia))
            {
                return Forbid();
            }

            return View();
        }

        public async Task<ActionResult<IEnumerable<IFileStoreEntry>>> GetFolders(string path)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageOwnMedia))
            {
                return Forbid();
            }

            if (string.IsNullOrEmpty(path))
            {
                path = "";
            }

            if (await _mediaFileStore.GetDirectoryInfoAsync(path) == null)
            {
                return NotFound();
            }

            var content = (await _mediaFileStore.GetDirectoryContentAsync(path)).Where(x => x.IsDirectory);

            var allowed = new List<IFileStoreEntry>();

            foreach (var entry in content)
            {
                if ((await _authorizationService.AuthorizeAsync(User, Permissions.ManageAttachedMediaFieldsFolder, (object)entry.Path)))
                {
                    allowed.Add(entry);
                }
            }

            return allowed.ToArray();
        }

        public async Task<ActionResult<IEnumerable<object>>> GetMediaItems(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = "";
            }

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageOwnMedia)
                || !await _authorizationService.AuthorizeAsync(User, Permissions.ManageAttachedMediaFieldsFolder, (object)path))
            {
                return Forbid();
            }

            if (await _mediaFileStore.GetDirectoryInfoAsync(path) == null)
            {
                return NotFound();
            }

            var files = (await _mediaFileStore.GetDirectoryContentAsync(path)).Where(x => !x.IsDirectory);

            var allowed = new List<IFileStoreEntry>();
            foreach (var entry in files)
            {
                if ((await _authorizationService.AuthorizeAsync(User, Permissions.ManageAttachedMediaFieldsFolder, (object)entry.Path)))
                {
                    allowed.Add(entry);
                }
            }

            return allowed.Select(CreateFileResult).ToArray();
        }

        public async Task<ActionResult<object>> GetMediaItem(string path)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageOwnMedia))
            {
                return Forbid();
            }

            if (string.IsNullOrEmpty(path))
            {
                return NotFound();
            }

            var f = await _mediaFileStore.GetFileInfoAsync(path);

            if (f == null)
            {
                return NotFound();
            }

            return CreateFileResult(f);
        }

        [HttpPost]
        [MediaSizeLimit]
        public async Task<ActionResult<object>> Upload(
            string path,
            string contentType,
            ICollection<IFormFile> files)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageOwnMedia))
            {
                return Forbid();
            }

            if (string.IsNullOrEmpty(path))
            {
                path = "";
            }

            var result = new List<object>();

            // Loop through each file in the request
            foreach (var file in files)
            {
                // TODO: support clipboard

                if (!_allowedFileExtensions.Contains(Path.GetExtension(file.FileName), StringComparer.OrdinalIgnoreCase))
                {
                    result.Add(new
                    {
                        name = file.FileName,
                        size = file.Length,
                        folder = path,
                        error = S["This file extension is not allowed: {0}", Path.GetExtension(file.FileName)].ToString()
                    });

                    _logger.LogInformation("File extension not allowed: '{0}'", file.FileName);

                    continue;
                }

                Stream stream = null;
                try
                {
                    var mediaFilePath = _mediaFileStore.Combine(path, file.FileName);
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
                        name = file.FileName,
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

            return new { files = result.ToArray() };
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFolder(string path)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageOwnMedia)
                || !await _authorizationService.AuthorizeAsync(User, Permissions.ManageAttachedMediaFieldsFolder, (object)path))
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
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageOwnMedia)
                || !await _authorizationService.AuthorizeAsync(User, Permissions.ManageAttachedMediaFieldsFolder, (object)path))
            {
                return Forbid();
            }

            if (string.IsNullOrEmpty(path))
            {
                return NotFound();
            }

            if (await _mediaFileStore.TryDeleteFileAsync(path) == false)
                return NotFound();

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> MoveMedia(string oldPath, string newPath)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageOwnMedia)
                || !await _authorizationService.AuthorizeAsync(User, Permissions.ManageAttachedMediaFieldsFolder, (object)oldPath))
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

            if (!_allowedFileExtensions.Contains(Path.GetExtension(newPath), StringComparer.OrdinalIgnoreCase))
            {
                return StatusCode(StatusCodes.Status403Forbidden, S["This file extension is not allowed: {0}", Path.GetExtension(newPath)]);
            }

            if (await _mediaFileStore.GetFileInfoAsync(newPath) != null)
            {
                return StatusCode(StatusCodes.Status403Forbidden, S["Cannot move media because a file already exists with the same name"]);
            }

            await _mediaFileStore.MoveFileAsync(oldPath, newPath);

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMediaList(string[] paths)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageOwnMedia))
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

            if (paths == null)
            {
                return NotFound();
            }

            foreach (var p in paths)
            {
                if (await _mediaFileStore.TryDeleteFileAsync(p) == false)
                    return NotFound();
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> MoveMediaList(string[] mediaNames, string sourceFolder, string targetFolder)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageOwnMedia)
                || !await _authorizationService.AuthorizeAsync(User, Permissions.ManageAttachedMediaFieldsFolder, (object)sourceFolder)
                || !await _authorizationService.AuthorizeAsync(User, Permissions.ManageAttachedMediaFieldsFolder, (object)targetFolder))
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
            else
            {
                return Ok();
            }
        }

        [HttpPost]
        public async Task<ActionResult<IFileStoreEntry>> CreateFolder(
            string path, string name,
            [FromServices] IAuthorizationService authorizationService)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = "";
            }

            var newPath = _mediaFileStore.Combine(path, name);

            if (!await authorizationService.AuthorizeAsync(User, Permissions.ManageOwnMedia)
                || !await authorizationService.AuthorizeAsync(User, Permissions.ManageAttachedMediaFieldsFolder, (object)newPath))
            {
                return Forbid();
            }

            var mediaFolder = await _mediaFileStore.GetDirectoryInfoAsync(newPath);
            if (mediaFolder != null)
            {
                return StatusCode(StatusCodes.Status403Forbidden, S["Cannot create folder because a folder already exists with the same name"]);
            }

            var existingFile = await _mediaFileStore.GetFileInfoAsync(newPath);
            if (existingFile != null)
            {
                return StatusCode(StatusCodes.Status403Forbidden, S["Cannot create folder because a file already exists with the same name"]);
            }

            await _mediaFileStore.TryCreateDirectoryAsync(newPath);

            mediaFolder = await _mediaFileStore.GetDirectoryInfoAsync(newPath);

            return new ObjectResult(mediaFolder);
        }

        public IActionResult MediaApplication()
        {
            return View();
        }

        public object CreateFileResult(IFileStoreEntry mediaFile)
        {
            _contentTypeProvider.TryGetContentType(mediaFile.Name, out var contentType);

            return new
            {
                name = mediaFile.Name,
                size = mediaFile.Length,
                folder = mediaFile.DirectoryPath,
                url = _mediaFileStore.MapPathToPublicUrl(mediaFile.Path),
                mediaPath = mediaFile.Path,
                mime = contentType ?? "application/octet-stream"
            };
        }
    }
}
