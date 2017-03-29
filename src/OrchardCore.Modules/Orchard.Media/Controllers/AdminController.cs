using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Orchard.ContentManagement;
using Orchard.Media.Indexes;
using YesSql.Core.Services;

namespace Orchard.Media.Controllers
{
    public class AdminController : Controller
    {
        private readonly IMediaFileStore _mediaFileStore;

        public AdminController(IMediaFileStore mediaFileStore)
        {
            _mediaFileStore = mediaFileStore;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetFolders(string path, [FromServices] IAuthorizationService authorizationService)
        {
            if (!await authorizationService.AuthorizeAsync(User, Permissions.ManageOwnMedia))
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

        public async Task<IActionResult> GetMediaItems(string path, [FromServices] IAuthorizationService authorizationService, [FromServices] YesSql.Core.Services.ISession session)
        {
            if (!await authorizationService.AuthorizeAsync(User, Permissions.ManageOwnMedia))
            {
                return Unauthorized();
            }

            if (string.IsNullOrEmpty(path))
            {
                path = "";
            }

            var media = await session
                .QueryIndexAsync<MediaPartIndex>(x => x.Folder == path.ToLowerInvariant())
                .OrderBy(x => x.FileName)
                .List();

            return Json(media.ToArray());
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<ActionResult> Upload(
            string path,
            string contentType,
            ICollection<IFormFile> files,
            [FromServices] IMediaService mediaService,
            [FromServices] IAuthorizationService authorizationService,
            [FromServices] IContentManager contentManager)
        {
            if (!await authorizationService.AuthorizeAsync(User, Permissions.ManageOwnMedia))
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
                                error = "Couldn't copy the file in the media store"
                            });
                        }
                    }

                    var media = await mediaService.ImportMediaAsync(mediaFilePath, file.ContentType, contentType);
                    contentManager.Create(media.ContentItem);

                    var mediaFile = await _mediaFileStore.GetFileAsync(mediaFilePath);

                    result.Add(new
                    {
                        name = mediaFile.Name,
                        size = mediaFile.Length,
                        url = mediaFile.AbsolutePath,
                        id = media.ContentItem.ContentItemId,
                        model = new MediaPartIndex
                        {
                            FileName = mediaFile.Name,
                            Folder = mediaFile.Folder,
                            Length = mediaFile.Length,
                            MimeType = file.ContentType
                        }
                    });
                }
                catch (Exception ex)
                {
                    result.Add(new
                    {
                        name = file.FileName,
                        size = file.Length,
                        error = ex.Message
                    });
                }
            }

            return Json(new { files = result.ToArray() });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFolder(
            string path,
            [FromServices] IAuthorizationService authorizationService,
            [FromServices] YesSql.Core.Services.ISession session,
            [FromServices] IContentManager contentManager)
        {
            if (!await authorizationService.AuthorizeAsync(User, Permissions.ManageOwnMedia))
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

            var mediaItems = await session.QueryAsync<ContentItem, MediaPartIndex>(x => x.Folder.StartsWith(path.ToLowerInvariant())).List();
            foreach (var mediaItem in mediaItems)
            {
                if (await authorizationService.AuthorizeAsync(User, Permissions.ManageOwnMedia, mediaItem))
                {
                    await contentManager.RemoveAsync(mediaItem);
                }
            }

            await _mediaFileStore.TryDeleteFolderAsync(path);

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
                return StatusCode(StatusCodes.Status403Forbidden, "Folder already exist");
            }

            await _mediaFileStore.TryCreateFolderAsync(newPath);

            mediaFolder = await _mediaFileStore.GetFolderAsync(newPath);

            return Json(mediaFolder);
        }
    }
}
