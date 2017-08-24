using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Orchard.StorageProviders;

namespace Orchard.Media.Controllers
{
    public class AdminController : Controller
    {
        private readonly IMediaFileStore _mediaFileStore;
        private readonly IAuthorizationService _authorizationService;
        private readonly ILogger _logger;

        public AdminController(
            IMediaFileStore mediaFileStore,
            IAuthorizationService authorizationService,
            ILogger<AdminController> logger)
        {
            _mediaFileStore = mediaFileStore;
            _authorizationService = authorizationService;
            _logger = logger;
        }

        public IActionResult Index()
        {
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

            var f = await _mediaFileStore.GetFileAsync(path);

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
                        if (!await _mediaFileStore.TrySaveStreamAsync(mediaFilePath, stream))
                        {
                            result.Add(new
                            {
                                name = file.FileName,
                                size = file.Length,
                                folder = path,
                                error = "Couldn't copy the file in the media store"
                            });
                        }
                    }

                    var mediaFile = await _mediaFileStore.GetFileAsync(mediaFilePath);

                    result.Add(CreateFileResult(mediaFile));
                }
                catch (Exception ex)
                {
                    _logger.LogError("An error occured while uploading a media: " + ex.Message);

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
                return StatusCode(StatusCodes.Status403Forbidden, "Cannot delete root media folder");
            }

            var mediaFolder = await _mediaFileStore.GetFolderAsync(path);

            if (mediaFolder == null || !mediaFolder.IsDirectory)
            {
                return StatusCode(StatusCodes.Status403Forbidden, "Cannot delete path");
            }

            await _mediaFileStore.TryDeleteFolderAsync(path);

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

            var media = await _mediaFileStore.GetFileAsync(path);

            if (media == null || media.IsDirectory)
            {
                return NotFound();
            }

            await _mediaFileStore.TryDeleteFileAsync(path);

            return Ok();
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

            var mediaFolder = await _mediaFileStore.GetFolderAsync(path);

            if (mediaFolder == null || !mediaFolder.IsDirectory)
            {
                return StatusCode(StatusCodes.Status404NotFound, "Cannot find path");
            }

            var newPath = _mediaFileStore.Combine(path, name);

            mediaFolder = await _mediaFileStore.GetFolderAsync(newPath);

            if (mediaFolder != null)
            {
                return StatusCode(StatusCodes.Status403Forbidden, "Folder already exists");
            }

            await _mediaFileStore.TryCreateFolderAsync(newPath);

            mediaFolder = await _mediaFileStore.GetFolderAsync(newPath);

            return Json(mediaFolder);
        }

        public IActionResult MediaApplication()
        {
            return View();
        }

        public object CreateFileResult(IFile mediaFile)
        {
            return new
            {
                name = mediaFile.Name,
                size = mediaFile.Length,
                folder = mediaFile.Folder,
                url = mediaFile.AbsolutePath,
                mediaPath = mediaFile.Path,
                mime = MimeMapping.MimeUtility.GetMimeMapping(mediaFile.Path)
            };
        }
    }
}
