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

namespace OrchardCore.Media.Services
{
    /// <summary>
    /// Handles file management operations related to the attached media field editor
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


        public async Task HandleFilesOnFieldUpdateAsync(List<EditMediaFieldItemInfo> items, string contentItemId, string contentType)
        {
            if (items == null)
            {
                throw new System.ArgumentNullException(nameof(items));
            }

            await EnsureGlobalDirectoriesExist();

            RemoveTemporary(items);

            MoveNewToContentItemDirAndUpdatePaths(items, contentItemId, contentType);
        }

        private async Task EnsureGlobalDirectoriesExist()
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
        private void MoveNewToContentItemDirAndUpdatePaths(List<EditMediaFieldItemInfo> items, string contentItemId, string contentType)
        {
            items.Where(x => !x.IsRemoved && x.IsNew).ToList()
                .ForEach(async x =>
                {
                    var targetDir = _fileStore.Combine(MediaFieldsFolder, contentType, contentItemId);
                    var finalFileName = (await GetFileHashAsync(x.Path)) + GetFileExtension(x.Path);
                    var finalFilePath = _fileStore.Combine(targetDir, finalFileName);

                    await _fileStore.TryCreateDirectoryAsync(targetDir);

                    // we don't move it if the same exact file is already there. To preserve disk space.
                    if (await _fileStore.GetFileInfoAsync(finalFilePath) == null)
                    {
                        await _fileStore.MoveFileAsync(x.Path, finalFilePath);
                    }

                    // update the path
                    x.Path = finalFilePath;
                });
        }

        
        private async Task<string> GetFileHashAsync(string filePath)
        {
            using (var fs = await _fileStore.GetFileStreamAsync(filePath))
            using (HashAlgorithm hashAlgorithm = MD5.Create())
            {
                byte[] hash = hashAlgorithm.ComputeHash(fs);
                return ByteArrayToHexString(hash);
            }
        }

        public string ByteArrayToHexString(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in bytes)
                sb.Append(b.ToString("x2").ToLower());

            return sb.ToString();
        }

        private string GetFileExtension(string path)
        {
            var lastPoint = path.LastIndexOf(".");
            return lastPoint > -1 ? path.Substring(lastPoint) : "";
        }
    }
}
