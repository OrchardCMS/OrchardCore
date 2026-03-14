using System.IO.Hashing;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement;
using OrchardCore.ContentPreview;
using OrchardCore.FileStorage;
using OrchardCore.Media.ViewModels;

namespace OrchardCore.Media.Services;

/// <summary>
/// Handles file management operations related to the attached media field.
/// </summary>
public class AttachedMediaFieldFileService
{
    private readonly IMediaFileStore _fileStore;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserAssetFolderNameProvider _userAssetFolderNameProvider;

    public AttachedMediaFieldFileService(
        IMediaFileStore fileStore,
        IHttpContextAccessor httpContextAccessor,
        IUserAssetFolderNameProvider userAssetFolderNameProvider)
    {
        _fileStore = fileStore;
        _httpContextAccessor = httpContextAccessor;
        _userAssetFolderNameProvider = userAssetFolderNameProvider;

        MediaFieldsFolder = "mediafields";
        MediaFieldsTempSubFolder = _fileStore.Combine(MediaFieldsFolder, "temp");
    }

    public string MediaFieldsFolder { get; }

    public string MediaFieldsTempSubFolder { get; }

    /// <summary>
    /// Copies the files to a folder specific for the content item.
    /// </summary>
    /// <param name="paths">The paths of the files to copy.</param>
    /// <param name="contentItem">The content item to which the files belong.</param>
    /// <returns>The updated paths of the copied files.</returns>
    public async Task<string[]> CopyFilesAsync(string[] paths, ContentItem contentItem)
    {
        var updatedPaths = paths;
        for (var i = 0; i < paths.Length; i++)
        {
            var path = paths[i];
            if (string.IsNullOrEmpty(path))
            {
                continue;
            }

            var targetDir = GetContentItemFolder(contentItem);
            var finalFileName = (await GetFileHashAsync(path)) + GetFileExtension(path);
            var finalFilePath = _fileStore.Combine(targetDir, finalFileName);

            await _fileStore.TryCreateDirectoryAsync(targetDir);

            if (await _fileStore.GetFileInfoAsync(finalFilePath) is null)
            {
                await _fileStore.CopyFileAsync(path, finalFilePath);

                updatedPaths[i] = finalFilePath;
            }
        }

        return updatedPaths;
    }

    /// <summary>
    /// Removes the assets attached to a content item through an attached media field.
    /// </summary>
    /// <remarks>
    /// To be used by content handlers when the content item is deleted.
    /// </remarks>
    public Task DeleteContentItemFolderAsync(ContentItem contentItem)
    {
        return _fileStore.TryDeleteDirectoryAsync(GetContentItemFolder(contentItem));
    }

    /// <summary>
    /// Moves uploaded files to a folder specific for the content item.
    /// </summary>
    public async Task HandleFilesOnFieldUpdateAsync(List<EditMediaFieldItemInfo> items, ContentItem contentItem)
    {
        ArgumentNullException.ThrowIfNull(items);

        if (_httpContextAccessor.HttpContext?.Features.Get<ContentPreviewFeature>()?.Previewing == true)
        {
            return;
        }

        await EnsureGlobalDirectoriesAsync();

        await RemoveTemporaryAsync(items);

        await MoveNewFilesToContentItemDirAndUpdatePathsAsync(items, contentItem);
    }

    /// <summary>
    /// Gets the per-user temporary upload directory.
    /// </summary>
    /// <returns></returns>
    public string GetMediaFieldsTempSubFolder()
        => _fileStore.Combine(MediaFieldsTempSubFolder, _userAssetFolderNameProvider.GetUserAssetFolderName(_httpContextAccessor.HttpContext.User));

    private async Task EnsureGlobalDirectoriesAsync()
    {
        await _fileStore.TryCreateDirectoryAsync(MediaFieldsFolder);
        await _fileStore.TryCreateDirectoryAsync(MediaFieldsTempSubFolder);
    }

    // Files just uploaded and then immediately discarded.
    private async Task RemoveTemporaryAsync(List<EditMediaFieldItemInfo> items)
    {
        foreach (var item in items.Where(i => i.IsRemoved && i.IsNew))
        {
            await _fileStore.TryDeleteFileAsync(item.Path);
        }
    }

    // Newly added files
    private async Task MoveNewFilesToContentItemDirAndUpdatePathsAsync(List<EditMediaFieldItemInfo> items, ContentItem contentItem)
    {
        // Copy to a list to allow removing files from the original items argument.
        var itemToParse = items.Where(i => !i.IsRemoved && !string.IsNullOrEmpty(i.Path)).ToList();
        foreach (var item in itemToParse)
        {
            var fileInfo = await _fileStore.GetFileInfoAsync(item.Path);

            if (fileInfo == null)
            {
                // File not found — keep the reference so the user can see which files are missing.
                continue;
            }

            var targetDir = GetContentItemFolder(contentItem);
            var finalFileName = (await GetFileHashAsync(item.Path)) + GetFileExtension(item.Path);
            var finalFilePath = _fileStore.Combine(targetDir, finalFileName);

            await _fileStore.TryCreateDirectoryAsync(targetDir);

            // When there is a validation error before creating the content item we can end up with an empty folder
            // because the content item is different on each form submit . We need to remove that empty folder.
            var previousDirPath = fileInfo.DirectoryPath;

            // finalFileName is a hash of the file. We preserve disk space by reusing the file.
            if (await _fileStore.GetFileInfoAsync(finalFilePath) == null)
            {
                await _fileStore.MoveFileAsync(item.Path, finalFilePath);
            }

            item.Path = finalFilePath;

            await DeleteDirIfEmptyAsync(previousDirPath);
        }

    }

    private async Task<string> GetFileHashAsync(string filePath)
    {
        using var fs = await _fileStore.GetFileStreamAsync(filePath);
        var hash = new XxHash32();
        await hash.AppendAsync(fs);
        return Convert.ToHexString(hash.GetCurrentHash()).ToLowerInvariant();
    }

    private static string GetFileExtension(string path)
    {
        var lastPoint = path.LastIndexOf('.');
        return lastPoint > -1 ? path[lastPoint..] : "";
    }

    private async Task DeleteDirIfEmptyAsync(string previousDirPath)
    {
        if (await _fileStore.GetDirectoryInfoAsync(previousDirPath) == null)
        {
            return;
        }

        if (!(await _fileStore.GetDirectoryContentAsync(previousDirPath).AnyAsync()))
        {
            await _fileStore.TryDeleteDirectoryAsync(previousDirPath);
        }
    }

    internal string GetContentItemFolder(ContentItem contentItem)
        => _fileStore.Combine(MediaFieldsFolder, contentItem.ContentType, contentItem.ContentItemId);
}
