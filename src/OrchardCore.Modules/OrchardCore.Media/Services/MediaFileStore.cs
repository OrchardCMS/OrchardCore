using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OrchardCore.FileStorage;

namespace OrchardCore.Media.Services
{
    public class MediaFileStore : IMediaFileStore
    {
        private readonly IFileStore _fileStore;
        private readonly string _publicUrlBase;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MediaFileStore(
            IFileStore fileStore,
            string publicUrlBase,
            IHttpContextAccessor httpContextAccessor)
        {
            _fileStore = fileStore;
            _publicUrlBase = publicUrlBase;
            _httpContextAccessor = httpContextAccessor;
        }

        public MediaFileStore(IFileStore fileStore)
        {
            _fileStore = fileStore;
        }

        public Task<IFileStoreEntry> GetFileInfoAsync(string path)
        {
            return _fileStore.GetFileInfoAsync(path);
        }

        public Task<IFileStoreEntry> GetDirectoryInfoAsync(string path)
        {
            return _fileStore.GetDirectoryInfoAsync(path);
        }

        public Task<IEnumerable<IFileStoreEntry>> GetDirectoryContentAsync(string path = null, bool includeSubDirectories = false)
        {
            return _fileStore.GetDirectoryContentAsync(path, includeSubDirectories);
        }

        public Task<bool> TryCreateDirectoryAsync(string path)
        {
            return _fileStore.TryCreateDirectoryAsync(path);
        }

        public Task<bool> TryDeleteFileAsync(string path)
        {
            return _fileStore.TryDeleteFileAsync(path);
        }

        public Task<bool> TryDeleteDirectoryAsync(string path)
        {
            return _fileStore.TryDeleteDirectoryAsync(path);
        }

        public Task MoveFileAsync(string oldPath, string newPath)
        {
            return _fileStore.MoveFileAsync(oldPath, newPath);
        }

        //public Task MoveDirectoryAsync(string oldPath, string newPath)
        //{
        //    return _fileStore.MoveDirectoryAsync(oldPath, newPath);
        //}

        public Task CopyFileAsync(string srcPath, string dstPath)
        {
            return _fileStore.CopyFileAsync(srcPath, dstPath);
        }

        public Task<Stream> GetFileStreamAsync(string path)
        {
            return _fileStore.GetFileStreamAsync(path);
        }

        public Task CreateFileFromStream(string path, Stream inputStream, bool overwrite = false)
        {
            return _fileStore.CreateFileFromStream(path, inputStream, overwrite);
        }

        public string MapPathToPublicUrl(string path)
        {
            var pathBase = _httpContextAccessor?.HttpContext.Request.PathBase ?? new PathString(null);
            var publicUrl = new PathString(_publicUrlBase.TrimEnd('/') + "/" + this.NormalizePath(path));
            return pathBase.Add(publicUrl);
        }

        public string MapPublicUrlToPath(string publicUrl)
        {
            var pathBase = _httpContextAccessor?.HttpContext.Request.PathBase ?? new PathString(null);
            var publicUrlBase = pathBase.Add(_publicUrlBase);

            if (!publicUrl.StartsWith(publicUrlBase, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentOutOfRangeException(nameof(publicUrl), "The specified URL is not inside the URL scope of the file store.");
            }

            return publicUrl.Substring(publicUrlBase.Value.Length);
        }
    }
}
