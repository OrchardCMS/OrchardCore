using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.FileStorage;
using OrchardCore.Media.Hubs;
using OrchardCore.Media.Services;
using OrchardCore.Media.ViewModels;

namespace OrchardCore.Media.Controllers;

[ApiController]
[Route("api/media-gen2")]
[Authorize, IgnoreAntiforgeryToken]
public class MediaGen2ApiController : Controller
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
    private readonly IHubContext<MediaHub> _mediaHub;

    public MediaGen2ApiController(
        IMediaFileStore mediaFileStore,
        IMediaNameNormalizerService mediaNameNormalizerService,
        IAuthorizationService authorizationService,
        IContentTypeProvider contentTypeProvider,
        IOptions<MediaOptions> options,
        ILogger<MediaGen2ApiController> logger,
        IStringLocalizer<MediaGen2ApiController> stringLocalizer,
        IUserAssetFolderNameProvider userAssetFolderNameProvider,
        IChunkFileUploadService chunkFileUploadService,
        IHubContext<MediaHub> mediaHub
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
        _mediaHub = mediaHub;
    }

    [HttpGet]
    [Route("GetCapabilities")]
    [ProducesResponseType(typeof(FileStoreCapabilitiesDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<FileStoreCapabilitiesDto>> GetCapabilities()
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();

        if (!await _authorizationService.AuthorizeAsync(User, MediaPermissions.ManageMedia))
        {
            return this.ApiChallengeOrForbidForCookieAuth();
        }

        _logger.LogWarning("GetCapabilities — total: {TotalMs}ms", sw.ElapsedMilliseconds);

        return Ok(new FileStoreCapabilitiesDto
        {
            HasHierarchicalNamespace = _mediaFileStore.Capabilities.HasHierarchicalNamespace,
            SupportsAtomicMove = _mediaFileStore.Capabilities.SupportsAtomicMove,
        });
    }

    [HttpGet]
    [Route("GetDirectoryTree")]
    [ProducesResponseType(typeof(DirectoryTreeNodeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<DirectoryTreeNodeDto>> GetDirectoryTree()
    {
        if (!await _authorizationService.AuthorizeAsync(User, MediaPermissions.ManageMedia))
        {
            return this.ApiChallengeOrForbidForCookieAuth();
        }

        // Create default user folder if needed.
        if (await _authorizationService.AuthorizeAsync(User, MediaPermissions.ManageOwnMedia)
            && await _mediaFileStore.GetDirectoryInfoAsync(_mediaFileStore.Combine(_mediaOptions.AssetsUsersFolder, _userAssetFolderNameProvider.GetUserAssetFolderName(User))) == null)
        {
            await _mediaFileStore.TryCreateDirectoryAsync(_mediaFileStore.Combine(_mediaOptions.AssetsUsersFolder, _userAssetFolderNameProvider.GetUserAssetFolderName(User)));
        }

        var root = new DirectoryTreeNodeDto
        {
            Name = string.Empty,
            Path = string.Empty,
            Children = [],
        };

        await BuildDirectoryTreeAsync(string.Empty, root.Children);

        return Ok(root);
    }

    private async Task BuildDirectoryTreeAsync(string path, List<DirectoryTreeNodeDto> children)
    {
        await foreach (var entry in _mediaFileStore.GetDirectoriesAsync(path))
        {
            var node = new DirectoryTreeNodeDto
            {
                Name = entry.Name,
                Path = entry.Path,
                Children = [],
            };

            children.Add(node);

            await BuildDirectoryTreeAsync(entry.Path, node.Children);
            node.HasChildren = node.Children.Count > 0;
        }
    }

    [HttpGet]
    [Route("GetFolders")]
    [ProducesResponseType(typeof(PaginatedFoldersDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaginatedFoldersDto>> GetFolders(string path, int skip = 0, int take = 0)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();

        if (!await _authorizationService.AuthorizeAsync(User, MediaPermissions.ManageMedia))
        {
            return this.ApiChallengeOrForbidForCookieAuth();
        }

        if (String.IsNullOrEmpty(path))
        {
            path = String.Empty;
        }

        // Only check directory existence for non-root paths (root always exists).
        if (path.Length > 0 && await _mediaFileStore.GetDirectoryInfoAsync(path) == null)
        {
            return this.ApiNotFoundProblem();
        }

        var authMs = sw.ElapsedMilliseconds;
        sw.Restart();

        var isPaginated = take > 0;
        var page = new List<FileStoreEntryDto>();
        var hasMore = false;
        var authorizedCount = 0;

        await foreach (var entry in _mediaFileStore.GetDirectoriesAsync(path))
        {
            authorizedCount++;

            if (isPaginated)
            {
                // Skip entries before the requested offset.
                if (authorizedCount <= skip)
                {
                    continue;
                }

                // Collect up to 'take' entries for the page.
                if (page.Count < take)
                {
                    page.Add(CreateFolderResult(entry));
                }
                else
                {
                    // We found one more beyond the page — there are more folders.
                    hasMore = true;
                    break;
                }
            }
            else
            {
                page.Add(CreateFolderResult(entry));
            }
        }

        var listingMs = sw.ElapsedMilliseconds;
        sw.Restart();

        // Check HasChildren for the page only (not all folders).
        var hasChildrenTasks = page.Select(async folder =>
        {
            folder.HasChildren = await HasSubDirectoriesAsync(folder.DirectoryPath);
        });
        await Task.WhenAll(hasChildrenTasks);

        var hasChildrenMs = sw.ElapsedMilliseconds;
        _logger.LogWarning("GetFolders path={Path} skip={Skip} take={Take} — auth: {AuthMs}ms, listing: {ListingMs}ms, hasChildren: {HasChildrenMs}ms, folders: {Count}",
            path, skip, take, authMs, listingMs, hasChildrenMs, page.Count);

        return Ok(new PaginatedFoldersDto { Items = page, HasMore = hasMore });
    }

    private async Task<bool> HasSubDirectoriesAsync(string path)
    {
        await foreach (var _ in _mediaFileStore.GetDirectoriesAsync(path))
        {
            return true;
        }

        return false;
    }

    [HttpGet]
    [Route("GetMediaItems")]
    [ProducesResponseType(typeof(IEnumerable<FileStoreEntryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<FileStoreEntryDto>>> GetMediaItems(string path, string extensions)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();

        if (String.IsNullOrEmpty(path))
        {
            path = String.Empty;
        }

        if (!await _authorizationService.AuthorizeAsync(User, MediaPermissions.ManageMedia))
        {
            return this.ApiChallengeOrForbidForCookieAuth();
        }

        var authMs = sw.ElapsedMilliseconds;

        // Only check directory existence for non-root paths (root always exists).
        if (path.Length > 0 && await _mediaFileStore.GetDirectoryInfoAsync(path) == null)
        {
            return this.ApiNotFoundProblem();
        }

        var dirCheckMs = sw.ElapsedMilliseconds - authMs;
        sw.Restart();

        var allowedExtensions = GetRequestedExtensions(extensions, false);

        var allowed = new List<FileStoreEntryDto>();

        await foreach (var entry in _mediaFileStore.GetFilesAsync(path))
        {
            if (allowedExtensions.Count == 0 || allowedExtensions.Contains(Path.GetExtension(entry.Path)))
            {
                allowed.Add(CreateFileResult(entry));
            }
        }

        _logger.LogWarning("GetMediaItems path={Path} — auth: {AuthMs}ms, dirCheck: {DirCheckMs}ms, listing: {ListingMs}ms, files: {Count}",
            path, authMs, dirCheckMs, sw.ElapsedMilliseconds, allowed.Count);

        return Ok(allowed);
    }

    /// <summary>
    /// Returns both subfolders and files for a directory in a single request,
    /// avoiding duplicate pipeline overhead from separate GetFolders + GetMediaItems calls.
    /// </summary>
    [HttpGet]
    [Route("GetDirectoryContent")]
    [ProducesResponseType(typeof(DirectoryContentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DirectoryContentDto>> GetDirectoryContent(string path, string extensions)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();

        if (String.IsNullOrEmpty(path))
        {
            path = String.Empty;
        }

        if (!await _authorizationService.AuthorizeAsync(User, MediaPermissions.ManageMedia))
        {
            return this.ApiChallengeOrForbidForCookieAuth();
        }

        // Only check directory existence for non-root paths (root always exists).
        if (path.Length > 0 && await _mediaFileStore.GetDirectoryInfoAsync(path) == null)
        {
            return this.ApiNotFoundProblem();
        }

        var authMs = sw.ElapsedMilliseconds;
        sw.Restart();

        // Fetch folders and files concurrently.
        var foldersTask = GetDirectoryFoldersAsync(path);
        var filesTask = GetDirectoryFilesAsync(path, extensions);

        await Task.WhenAll(foldersTask, filesTask);

        var listingMs = sw.ElapsedMilliseconds;

        _logger.LogWarning("GetDirectoryContent path={Path} — auth: {AuthMs}ms, listing: {ListingMs}ms, folders: {FolderCount}, files: {FileCount}",
            path, authMs, listingMs, foldersTask.Result.Count, filesTask.Result.Count);

        return Ok(new DirectoryContentDto
        {
            Folders = foldersTask.Result,
            Files = filesTask.Result,
        });
    }

    private async Task<List<FileStoreEntryDto>> GetDirectoryFoldersAsync(string path)
    {
        var folders = new List<FileStoreEntryDto>();

        await foreach (var entry in _mediaFileStore.GetDirectoriesAsync(path))
        {
            folders.Add(CreateFolderResult(entry));
        }

        // Check HasChildren concurrently.
        var hasChildrenTasks = folders.Select(async folder =>
        {
            folder.HasChildren = await HasSubDirectoriesAsync(folder.DirectoryPath);
        });
        await Task.WhenAll(hasChildrenTasks);

        return folders;
    }

    private async Task<List<FileStoreEntryDto>> GetDirectoryFilesAsync(string path, string extensions)
    {
        var allowedExtensions = GetRequestedExtensions(extensions, false);
        var files = new List<FileStoreEntryDto>();

        await foreach (var entry in _mediaFileStore.GetFilesAsync(path))
        {
            if (allowedExtensions.Count == 0 || allowedExtensions.Contains(Path.GetExtension(entry.Path)))
            {
                files.Add(CreateFileResult(entry));
            }
        }

        return files;
    }

    [HttpGet]
    [Route("GetMediaItem")]
    [ProducesResponseType(typeof(FileStoreEntryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FileStoreEntryDto>> GetMediaItem(string path)
    {
        if (!await _authorizationService.AuthorizeAsync(User, MediaPermissions.ManageMedia))
        {
            return this.ApiChallengeOrForbidForCookieAuth();
        }

        if (String.IsNullOrEmpty(path))
        {
            return this.ApiNotFoundProblem();
        }

        var fileEntry = await _mediaFileStore.GetFileInfoAsync(path);

        if (fileEntry == null)
        {
            return this.ApiNotFoundProblem();
        }

        return CreateFileResult(fileEntry);
    }

    [HttpGet]
    [Route("GetAllMediaItems")]
    [ProducesResponseType(typeof(IEnumerable<FileStoreEntryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<FileStoreEntryDto>>> GetAllMediaItems(string extensions)
    {
        if (!await _authorizationService.AuthorizeAsync(User, MediaPermissions.ManageMedia))
        {
            return this.ApiChallengeOrForbidForCookieAuth();
        }

        // create default folders if not exist
        if (await _authorizationService.AuthorizeAsync(User, MediaPermissions.ManageOwnMedia)
            && await _mediaFileStore.GetDirectoryInfoAsync(_mediaFileStore.Combine(_mediaOptions.AssetsUsersFolder, _userAssetFolderNameProvider.GetUserAssetFolderName(User))) == null)
        {
            await _mediaFileStore.TryCreateDirectoryAsync(_mediaFileStore.Combine(_mediaOptions.AssetsUsersFolder, _userAssetFolderNameProvider.GetUserAssetFolderName(User)));
        }

        var allowedExtensions = GetRequestedExtensions(extensions, false);
        var allItems = new List<FileStoreEntryDto>();

        await CollectAllItemsRecursiveAsync(String.Empty, allowedExtensions, allItems);

        return Ok(allItems);
    }

    private async Task CollectAllItemsRecursiveAsync(string path, HashSet<string> allowedExtensions, List<FileStoreEntryDto> allItems)
    {
        var subFolders = new List<IFileStoreEntry>();

        await foreach (var entry in _mediaFileStore.GetDirectoryContentAsync(path))
        {
            if (entry.IsDirectory)
            {
                allItems.Add(CreateFolderResult(entry));
                subFolders.Add(entry);
            }
            else if (allowedExtensions.Count == 0 || allowedExtensions.Contains(Path.GetExtension(entry.Path)))
            {
                allItems.Add(CreateFileResult(entry));
            }
        }

        foreach (var folder in subFolders)
        {
            await CollectAllItemsRecursiveAsync(folder.Path, allowedExtensions, allItems);
        }
    }

    [HttpPost]
    [MediaSizeLimit]
    [Route("Upload")]
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
            return this.ApiChallengeOrForbidForCookieAuth();
        }

        var allowedExtensions = GetRequestedExtensions(extensions, true);

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
                            error = ex.Message,
                        });
                    }
                    finally
                    {
                        stream?.Dispose();
                    }
                }

                // Broadcast file upload event via SignalR (no IMediaEventHandler for file creation).
                await _mediaHub.Clients.All.SendAsync("MediaChanged", new
                {
                    action = "fileUploaded",
                    path,
                });

                return Ok(new { files = result.ToArray() });
            });
    }

    [HttpPost]
    [Route("CopyMedia")]
    [ProducesResponseType(typeof(FileStoreEntryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FileStoreEntryDto>> CopyMedia(string oldPath, string newPath)
    {
        if (!await _authorizationService.AuthorizeAsync(User, MediaPermissions.ManageMedia)
            || !await _authorizationService.AuthorizeAsync(User, MediaPermissions.ManageMediaFolder, (object)oldPath)
            || !await _authorizationService.AuthorizeAsync(User, MediaPermissions.ManageMediaFolder, (object)newPath))
        {
            return this.ApiChallengeOrForbidForCookieAuth();
        }

        if (String.IsNullOrEmpty(oldPath) || String.IsNullOrEmpty(newPath))
        {
            return this.ApiNotFoundProblem();
        }

        if (await _mediaFileStore.GetFileInfoAsync(oldPath) == null)
        {
            return this.ApiNotFoundProblem();
        }

        var newExtension = Path.GetExtension(newPath);

        if (!_mediaOptions.AllowedFileExtensions.Contains(newExtension, StringComparer.OrdinalIgnoreCase))
        {
            return this.ApiValidationProblem(detail: S["This file extension is not allowed: {0}", newExtension]);
        }

        if (await _mediaFileStore.GetFileInfoAsync(newPath) != null)
        {
            return this.ApiValidationProblem(detail: S["Cannot copy media because a file already exists with the same name"]);
        }

        await _mediaFileStore.CopyFileAsync(oldPath, newPath);

        var copiedFile = await _mediaFileStore.GetFileInfoAsync(newPath);

        // Broadcast file copy event via SignalR (no IMediaEventHandler for file copy).
        await _mediaHub.Clients.All.SendAsync("MediaChanged", new
        {
            action = "fileCopied",
            path = oldPath,
            newPath,
        });

        return Ok(CreateFileResult(copiedFile));
    }

    [HttpPost]
    [Route("DeleteFolder")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteFolder(string path)
    {
        if (!await _authorizationService.AuthorizeAsync(User, MediaPermissions.ManageMedia)
            || !await _authorizationService.AuthorizeAsync(User, MediaPermissions.ManageMediaFolder, (object)path))
        {
            return this.ApiChallengeOrForbidForCookieAuth();
        }

        if (String.IsNullOrEmpty(path))
        {
            return this.ApiBadRequestProblem(detail: S["Cannot delete root media folder"]);
        }

        var mediaFolder = await _mediaFileStore.GetDirectoryInfoAsync(path);
        if (mediaFolder != null && !mediaFolder.IsDirectory)
        {
            return this.ApiBadRequestProblem(detail: S["Cannot delete path because it is not a directory"]);
        }

        if (await _mediaFileStore.TryDeleteDirectoryAsync(path) == false)
        {
            return this.ApiNotFoundProblem();
        }

        return Ok();
    }

    [HttpPost]
    [Route("DeleteMedia")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMedia(string path)
    {
        if (!await _authorizationService.AuthorizeAsync(User, MediaPermissions.ManageMedia)
            || !await _authorizationService.AuthorizeAsync(User, MediaPermissions.ManageMediaFolder, (object)path))
        {
            return this.ApiChallengeOrForbidForCookieAuth();
        }

        if (String.IsNullOrEmpty(path))
        {
            return this.ApiNotFoundProblem();
        }

        if (!await _mediaFileStore.TryDeleteFileAsync(path))
        {
            return this.ApiNotFoundProblem();
        }

        return Ok();
    }

    [HttpPost]
    [Route("MoveMedia")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MoveMedia(string oldPath, string newPath)
    {
        if (!await _authorizationService.AuthorizeAsync(User, MediaPermissions.ManageMedia)
            || !await _authorizationService.AuthorizeAsync(User, MediaPermissions.ManageMediaFolder, (object)oldPath)
            || !await _authorizationService.AuthorizeAsync(User, MediaPermissions.ManageMediaFolder, (object)newPath))
        {
            return this.ApiChallengeOrForbidForCookieAuth();
        }

        if (String.IsNullOrEmpty(oldPath) || String.IsNullOrEmpty(newPath))
        {
            return this.ApiNotFoundProblem();
        }

        if (await _mediaFileStore.GetFileInfoAsync(oldPath) == null)
        {
            return this.ApiNotFoundProblem();
        }

        var newExtension = Path.GetExtension(newPath);

        if (!_mediaOptions.AllowedFileExtensions.Contains(newExtension, StringComparer.OrdinalIgnoreCase))
        {
            return this.ApiValidationProblem(detail: S["This file extension is not allowed: {0}", newExtension]);
        }

        if (await _mediaFileStore.GetFileInfoAsync(newPath) != null)
        {
            return this.ApiValidationProblem(detail: S["Cannot move media because a file already exists with the same name"]);
        }

        await _mediaFileStore.MoveFileAsync(oldPath, newPath);

        return Ok();
    }

    [HttpPost]
    [Route("DeleteMediaList")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMediaList([FromBody] List<String> paths)
    {
        if (paths == null)
        {
            return this.ApiNotFoundProblem();
        }

        if (!await _authorizationService.AuthorizeAsync(User, MediaPermissions.ManageMedia))
        {
            return this.ApiChallengeOrForbidForCookieAuth();
        }

        foreach (var path in paths)
        {
            if (!await _authorizationService.AuthorizeAsync(User, MediaPermissions.ManageMediaFolder, (object)path))
            {
                return this.ApiChallengeOrForbidForCookieAuth();
            }
        }

        foreach (var path in paths)
        {
            if (!await _mediaFileStore.TryDeleteFileAsync(path))
            {
                return this.ApiNotFoundProblem();
            }
        }

        return Ok();
    }

    [HttpPost]
    [Route("MoveMediaList")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MoveMediaList([FromBody] MoveMedias model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, MediaPermissions.ManageMedia)
            || !await _authorizationService.AuthorizeAsync(User, MediaPermissions.ManageMediaFolder, (object)model.sourceFolder)
            || !await _authorizationService.AuthorizeAsync(User, MediaPermissions.ManageMediaFolder, (object)model.targetFolder))
        {
            return this.ApiChallengeOrForbidForCookieAuth();
        }

        if ((model.mediaNames == null) || (model.mediaNames.Length < 1)
            || String.IsNullOrEmpty(model.sourceFolder)
            || String.IsNullOrEmpty(model.targetFolder))
        {
            return this.ApiNotFoundProblem();
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
            return this.ApiValidationProblem(detail: S["Error when moving files. Maybe they already exist on the target folder? Files on error: {0}", String.Join(",", filesOnError)]);
        }

        return Ok();
    }

    [HttpPost]
    [Route("CreateFolder")]
    [ProducesResponseType(typeof(FileStoreEntryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<FileStoreEntryDto>> CreateFolder(
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
            return this.ApiValidationProblem(detail: S["Cannot create folder because the folder name contains invalid characters"]);
        }

        var newPath = _mediaFileStore.Combine(path, name);

        if (!await authorizationService.AuthorizeAsync(User, MediaPermissions.ManageMedia)
            || !await authorizationService.AuthorizeAsync(User, MediaPermissions.ManageMediaFolder, (object)newPath))
        {
            return this.ApiChallengeOrForbidForCookieAuth();
        }

        var mediaFolder = await _mediaFileStore.GetDirectoryInfoAsync(newPath);
        if (mediaFolder != null)
        {
            return this.ApiValidationProblem(detail: S["Cannot create folder because a folder already exists with the same name"]);
        }

        var existingFile = await _mediaFileStore.GetFileInfoAsync(newPath);
        if (existingFile != null)
        {
            return this.ApiValidationProblem(detail: S["Cannot create folder because a file already exists with the same name"]);
        }

        await _mediaFileStore.TryCreateDirectoryAsync(newPath);

        mediaFolder = await _mediaFileStore.GetDirectoryInfoAsync(newPath);

        return new ObjectResult(CreateFolderResult(mediaFolder));
    }

    private FileStoreEntryDto CreateFileResult(IFileStoreEntry mediaFile)
    {
        _contentTypeProvider.TryGetContentType(mediaFile.Name, out var contentType);

        return new FileStoreEntryDto
        {
            Name = mediaFile.Name,
            Size = mediaFile.Length,
            DirectoryPath = mediaFile.DirectoryPath,
            FilePath = mediaFile.Path,
            LastModifiedUtc = mediaFile.LastModifiedUtc,
            IsDirectory = false,
            Url = _mediaFileStore.MapPathToPublicUrl(mediaFile.Path),
            Mime = contentType ?? "application/octet-stream",
        };
    }

    private static FileStoreEntryDto CreateFolderResult(IFileStoreEntry folder)
    {
        return new FileStoreEntryDto
        {
            Name = folder.Name,
            Size = folder.Length,
            DirectoryPath = folder.Path,
            LastModifiedUtc = folder.LastModifiedUtc,
            IsDirectory = true,
        };
    }

    [Route("Options")]
    public async Task<IActionResult> Options()
    {
        if (!await _authorizationService.AuthorizeAsync(User, MediaPermissions.ViewMediaOptions))
        {
            return Forbid();
        }

        return View(_mediaOptions);
    }

    [HttpGet]
    [Route("TusFileInfo/{uploadId}")]
    [ProducesResponseType(typeof(FileStoreEntryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FileStoreEntryDto>> GetTusFileInfo(
        string uploadId,
        [FromServices] TusUploadMetadataStore tusMetadataStore)
    {
        if (!await _authorizationService.AuthorizeAsync(User, MediaPermissions.ManageMedia))
        {
            return this.ApiChallengeOrForbidForCookieAuth();
        }

        var entry = tusMetadataStore.Get(uploadId);

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
        tusMetadataStore.Remove(uploadId);

        return Ok(CreateFileResult(fileInfo));
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

public class FileStoreEntryDto
{
    public string Name { get; set; }
    public long Size { get; set; }
    public string DirectoryPath { get; set; }
    public string FilePath { get; set; }
    public DateTime LastModifiedUtc { get; set; }
    public bool IsDirectory { get; set; }
    public string Url { get; set; }
    public string Mime { get; set; }
    public bool? HasChildren { get; set; }
}

public class FileStoreCapabilitiesDto
{
    public bool HasHierarchicalNamespace { get; set; }
    public bool SupportsAtomicMove { get; set; }
}

public class PaginatedFoldersDto
{
    public List<FileStoreEntryDto> Items { get; set; }
    public bool HasMore { get; set; }
}

public class DirectoryContentDto
{
    public List<FileStoreEntryDto> Folders { get; set; }
    public List<FileStoreEntryDto> Files { get; set; }
}

public class DirectoryTreeNodeDto
{
    public string Name { get; set; }
    public string Path { get; set; }
    public bool HasChildren { get; set; }
    public List<DirectoryTreeNodeDto> Children { get; set; }
}
