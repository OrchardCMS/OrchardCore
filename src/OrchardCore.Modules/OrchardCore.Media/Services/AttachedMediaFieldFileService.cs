using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.FileStorage;
using OrchardCore.Media.ViewModels;
using System.Security.Cryptography;
using System.IO;
using OrchardCore.ContentManagement;

namespace OrchardCore.Media.Services
{
    /// <summary>
    /// Handles file management operations related to the attached media field
    /// </summary>    
    public class AttachedMediaFieldFileService
    {
        private readonly IMediaFileStore _fileStore;
        private readonly IStringLocalizer<AttachedMediaFieldFileService> T;

        public AttachedMediaFieldFileService(IMediaFileStore fileStore,
            IStringLocalizer<AttachedMediaFieldFileService> stringLocalizer)
        {
            _fileStore = fileStore;
            T = stringLocalizer;

            MediaFieldsFolder = "mediafields";
            MediaFieldsTempSubFolder = _fileStore.Combine(MediaFieldsFolder, "temp");
        }

        public string MediaFieldsFolder { get; }
        public string MediaFieldsTempSubFolder { get; }

        /// <summary>
        /// Removes the assets attached to a content item through an attached media field.
        /// </summary>
        /// <remarks>
        /// To be used by content handlers when the content item is deleted
        /// </remarks>
        public async Task DeleteContentItemFolder(ContentItem contentItem)
        {
            await _fileStore.TryDeleteDirectoryAsync(GetContentItemFolder(contentItem));
        }


        /// <summary>
        /// Moves uploaded files to a folder specific for the content item
        /// </summary>
        public async Task HandleFilesOnFieldUpdateAsync(List<EditMediaFieldItemInfo> items, ContentItem contentItem)
        {
            if (items == null)
            {
                throw new System.ArgumentNullException(nameof(items));
            }

            await EnsureGlobalDirectoriesAsync();

            RemoveTemporary(items);

            MoveNewFilesToContentItemDirAndUpdatePaths(items, contentItem);
        }


        private async Task EnsureGlobalDirectoriesAsync()
        {
            await _fileStore.TryCreateDirectoryAsync(MediaFieldsFolder);
            await _fileStore.TryCreateDirectoryAsync(MediaFieldsTempSubFolder);
        }


        // Files just uploaded and then inmediately discarded.
        private void RemoveTemporary(List<EditMediaFieldItemInfo> items)
        {
            items.Where(x => x.IsRemoved && x.IsNew).ToList().ForEach(async x => await _fileStore.TryDeleteFileAsync(x.Path));
        }


        // Newly added files
        private void MoveNewFilesToContentItemDirAndUpdatePaths(List<EditMediaFieldItemInfo> items, ContentItem contentItem)
        {
            items.Where(x => !x.IsRemoved).ToList()
                .ForEach(async x =>
                {
                    var targetDir = GetContentItemFolder(contentItem);
                    var finalFileName = (await GetFileHashAsync(x.Path)) + GetFileExtension(x.Path);
                    var finalFilePath = _fileStore.Combine(targetDir, finalFileName);
                    
                    await _fileStore.TryCreateDirectoryAsync(targetDir);

                    // When there is a validation error before creating the content item we can end up with an empty folder
                    // because the content item is different on each form submit . We need to remove that empty folder.
                    var previousDirPath = (await _fileStore.GetFileInfoAsync(x.Path)).DirectoryPath;

                    // fileName is a hash of the file. We preserve disk space by reusing the file.
                    if (await _fileStore.GetFileInfoAsync(finalFilePath) == null)
                    {
                        await _fileStore.MoveFileAsync(x.Path, finalFilePath);
                    }

                    x.Path = finalFilePath;

                    await DeleteDirIfEmpty(previousDirPath);                    
                    
                });
        }


        private string GetContentItemFolder(ContentItem contentItem)
        {
            return _fileStore.Combine(MediaFieldsFolder, contentItem.ContentType, contentItem.ContentItemId);
        }


        private async Task<string> GetFileHashAsync(string filePath)
        {
            using (var fs = await _fileStore.GetFileStreamAsync(filePath))
            using (HashAlgorithm hashAlgorithm = MD5.Create())
            {
                var hash = hashAlgorithm.ComputeHash(fs);
                return ByteArrayToHexString(hash);
            }
        }


        public string ByteArrayToHexString(byte[] bytes)
        {
            var sb = new StringBuilder();
            foreach (var b in bytes)
            {
                sb.Append(b.ToString("x2").ToLower());
            }                

            return sb.ToString();
        }


        private string GetFileExtension(string path)
        {
            var lastPoint = path.LastIndexOf(".");
            return lastPoint > -1 ? path.Substring(lastPoint) : "";
        }


        private async Task DeleteDirIfEmpty(string previousDirPath)
        {
            if (await _fileStore.GetDirectoryInfoAsync(previousDirPath) == null)
            {
                return;
            }

            if (!(await _fileStore.GetDirectoryContentAsync(previousDirPath)).Any())
            {
                await _fileStore.TryDeleteDirectoryAsync(previousDirPath);
            }
        }
    }
}
