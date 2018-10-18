using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.FileStorage;

namespace OrchardCore.Media.Controllers
{
    public class AdminController : Controller
    {
        private readonly IMediaFileStore _mediaFileStore;
        private readonly IAuthorizationService _authorizationService;
        private readonly IContentTypeProvider _contentTypeProvider;
        private readonly ILogger _logger;
        private readonly IStringLocalizer<AdminController> T;

        public AdminController(
            IMediaFileStore mediaFileStore,
            IAuthorizationService authorizationService,
            IContentTypeProvider contentTypeProvider,
            ILogger<AdminController> logger,
            IStringLocalizer<AdminController> stringLocalizer)
        {
            _mediaFileStore = mediaFileStore;
            _authorizationService = authorizationService;
            _contentTypeProvider = contentTypeProvider;
            _logger = logger;
            T = stringLocalizer;
            
        }

        public async Task<IActionResult> Index()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageOwnMedia))
            {
                return Unauthorized();
            }

            return View();
        }

        public async Task<IActionResult> GetFolders(string path)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageOwnMedia))
            {
                return Unauthorized();
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

            return Json(content.ToArray());
        }

        public async Task<IActionResult> GetMediaItems(string path)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageOwnMedia))
            {
                return Unauthorized();
            }

            if (string.IsNullOrEmpty(path))
            {
                path = "";
            }

            if (await _mediaFileStore.GetDirectoryInfoAsync(path) == null)
            {
                return NotFound();
            }

            var files = (await _mediaFileStore.GetDirectoryContentAsync(path)).Where(x => !x.IsDirectory);

            return Json(files.Select(CreateFileResult).ToArray());
        }

        public async Task<IActionResult> GetMediaItem(string path)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageOwnMedia))
            {
                return Unauthorized();
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

            return Json(CreateFileResult(f));
        }

        [HttpPost]
        public async Task<ActionResult> Upload(
            string path,
            string contentType,
            ICollection<IFormFile> files)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageOwnMedia))
            {
                return Unauthorized();
            }

            if (string.IsNullOrEmpty(path))
            {
                path = "";
            }

            var result = new List<object>();

            // TODO: Validate file extensions

            // Loop through each file in the request
            foreach (var file in files)
            {
                // TODO: support clipboard

                try
                {
                    var mediaFilePath = _mediaFileStore.Combine(path, file.FileName);

                    using (var stream = file.OpenReadStream())
                    {
                        await _mediaFileStore.CreateFileFromStream(mediaFilePath, stream);
                    }

                    var mediaFile = await _mediaFileStore.GetFileInfoAsync(mediaFilePath);

                    result.Add(CreateFileResult(mediaFile));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occured while uploading a media");

                    result.Add(new
                    {
                        name = file.FileName,
                        size = file.Length,
                        folder = path,
                        error = ex.Message
                    });
                }
            }

            return Json(new { files = result.ToArray() });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFolder(string path)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageOwnMedia))
            {
                return Unauthorized();
            }

            if (string.IsNullOrEmpty(path))
            {
                return StatusCode(StatusCodes.Status403Forbidden, T["Cannot delete root media folder"]);
            }

            var mediaFolder = await _mediaFileStore.GetDirectoryInfoAsync(path);
            if (mediaFolder != null && !mediaFolder.IsDirectory)
            {
                return StatusCode(StatusCodes.Status403Forbidden, T["Cannot delete path because it is not a directory"]);
            }

            if (await _mediaFileStore.TryDeleteDirectoryAsync(path) == false)
                return NotFound();

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMedia(string path)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageOwnMedia))
            {
                return Unauthorized();
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
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageOwnMedia))
            {
                return Unauthorized();
            }

            if (string.IsNullOrEmpty(oldPath) || string.IsNullOrEmpty(newPath))
            {
                return NotFound();
            }

            if (await _mediaFileStore.GetFileInfoAsync(oldPath) == null)
            {
                return NotFound();
            }

            if (await _mediaFileStore.GetFileInfoAsync(newPath) != null)
            {
                return StatusCode(StatusCodes.Status403Forbidden, T["Cannot move media because a file already exists with the same name"]);
            }

            await _mediaFileStore.MoveFileAsync(oldPath, newPath);

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMediaList(string[] paths)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageOwnMedia))
            {
                return Unauthorized();
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
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageOwnMedia))
            {
                return Unauthorized();
            }

            if ((mediaNames == null) || (mediaNames.Length < 1) 
                || string.IsNullOrEmpty(sourceFolder)
                || string.IsNullOrEmpty(targetFolder))
            {
                return NotFound();
            }

            sourceFolder = sourceFolder == "root" ? string.Empty : sourceFolder;
            targetFolder = targetFolder == "root" ? string.Empty : targetFolder;

            var  filesOnError = new List<string>();

            foreach (var name in mediaNames)
            {
                var sourcePath = _mediaFileStore.Combine(sourceFolder, name);
                var targetPath = _mediaFileStore.Combine( targetFolder, name);
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
                return BadRequest(T["Error when moving files. Maybe they already exist on the target folder? Files on error: {0}", string.Join(",", filesOnError)].ToString());
            }
            else
            {
                return Ok();
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateFolder(
            string path, string name,
            [FromServices] IAuthorizationService authorizationService)
        {
            if (!await authorizationService.AuthorizeAsync(User, Permissions.ManageOwnMedia))
            {
                return Unauthorized();
            }

            if (string.IsNullOrEmpty(path))
            {
                path = "";
            }

            var newPath = _mediaFileStore.Combine(path, name);

            var mediaFolder = await _mediaFileStore.GetDirectoryInfoAsync(newPath);
            if (mediaFolder != null)
            {
                return StatusCode(StatusCodes.Status403Forbidden, T["Cannot create folder because a folder already exists with the same name"]);
            }

            var existingFile = await _mediaFileStore.GetFileInfoAsync(newPath);
            if (existingFile != null)
            {
                return StatusCode(StatusCodes.Status403Forbidden, T["Cannot create folder because a file already exists with the same name"]);
            }

            await _mediaFileStore.TryCreateDirectoryAsync(newPath);

            mediaFolder = await _mediaFileStore.GetDirectoryInfoAsync(newPath);

            return Json(mediaFolder);
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
