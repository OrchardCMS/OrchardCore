using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Orchard.StorageProviders;

namespace Orchard.Media
{
    public class MediaFileStore : IMediaFileStore
    {
        private readonly IFileStore _fileStore;

        public MediaFileStore(IFileStore fileStore)
        {
            _fileStore = fileStore;
        }

        public string Combine(params string[] paths)
        {
            return _fileStore.Combine(paths);
        }

        public Task<IEnumerable<IFile>> GetDirectoryContentAsync(string subpath = null)
        {
            return _fileStore.GetDirectoryContentAsync(subpath);
        }

        public Task<IFile> GetFileAsync(string subpath)
        {
            return _fileStore.GetFileAsync(subpath);
        }

        public Task<IFile> GetFolderAsync(string subpath)
        {
            return _fileStore.GetFolderAsync(subpath);
        }

        public string GetPublicUrl(string subpath)
        {
            return _fileStore.GetPublicUrl(subpath);
        }

        public Task<IFile> MapFileAsync(string absoluteUrl)
        {
            return _fileStore.MapFileAsync(absoluteUrl);
        }

        public Task<bool> TryCopyFileAsync(string originalPath, string duplicatePath)
        {
            return _fileStore.TryCopyFileAsync(originalPath, duplicatePath);
        }

        public Task<bool> TryCreateFolderAsync(string subpath)
        {
            return _fileStore.TryCreateFolderAsync(subpath);
        }

        public Task<bool> TryDeleteFileAsync(string subpath)
        {
            return _fileStore.TryDeleteFileAsync(subpath);
        }

        public Task<bool> TryDeleteFolderAsync(string subpath)
        {
            return _fileStore.TryDeleteFolderAsync(subpath);
        }

        public Task<bool> TryMoveFileAsync(string oldPath, string newPath)
        {
            return _fileStore.TryMoveFileAsync(oldPath, newPath);
        }

        public Task<bool> TryMoveFolderAsync(string oldPath, string newPath)
        {
            return _fileStore.TryMoveFolderAsync(oldPath, newPath);
        }

        public Task<bool> TrySaveStreamAsync(string subpath, Stream inputStream)
        {
            return _fileStore.TrySaveStreamAsync(subpath, inputStream);
        }
    }
}
