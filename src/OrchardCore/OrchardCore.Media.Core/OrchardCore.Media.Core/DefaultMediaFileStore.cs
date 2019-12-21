using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.FileStorage;
using OrchardCore.Media.Events;
using OrchardCore.Modules;

namespace OrchardCore.Media.Core
{
    public class DefaultMediaFileStore : IMediaFileStore
    {
        private readonly IFileStore _fileStore;
        private readonly string _requestBasePath;
        private readonly string _cdnBaseUrl;
        private readonly IEnumerable<IMediaEventHandler> _mediaEventHandlers;
        public DefaultMediaFileStore(
            ILogger<DefaultMediaFileStore> logger,
            IEnumerable<IMediaEventHandler> mediaEventHandlers,
            IFileStore fileStore,
            string requestBasePath,
            string cdnBaseUrl)
        {
            Logger = logger;
            _mediaEventHandlers = mediaEventHandlers;
            _fileStore = fileStore;

            // Ensure trailing slash removed.
            _requestBasePath = requestBasePath.TrimEnd('/');

            // Media options configuration ensures any trailing slash is removed.
            _cdnBaseUrl = cdnBaseUrl;
        }

        public ILogger Logger { get; }
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
            MediaRemoveContext mediaContext = new MediaRemoveContext
            {
                Path = path
            };
            _mediaEventHandlers.Invoke((handler, context) => handler.MediaDeleting(context), mediaContext, Logger);
            var task = _fileStore.TryDeleteFileAsync(path);            
            _mediaEventHandlers.Invoke((handler, context) => handler.MediaDeleted(context), mediaContext, Logger);
            return task;
        }

        public Task<bool> TryDeleteDirectoryAsync(string path)
        {
            return _fileStore.TryDeleteDirectoryAsync(path);
        }

        public Task MoveFileAsync(string oldPath, string newPath)
        {
            return _fileStore.MoveFileAsync(oldPath, newPath);
        }

        public Task CopyFileAsync(string srcPath, string dstPath)
        {
            return _fileStore.CopyFileAsync(srcPath, dstPath);
        }

        public Task<Stream> GetFileStreamAsync(string path)
        {
            return _fileStore.GetFileStreamAsync(path);
        }

        public Task<Stream> GetFileStreamAsync(IFileStoreEntry fileStoreEntry)
        {
            return _fileStore.GetFileStreamAsync(fileStoreEntry);
        }

        public Task CreateFileFromStreamAsync(string path, Stream inputStream, bool overwrite = false)
        {
            MediaCreationContext mediaContext = new MediaCreationContext
            {
                Path = path,
                Stream = inputStream
            };
            _mediaEventHandlers.Invoke((handler, context) => handler.MediaCreating(context), mediaContext, Logger);
            var task = _fileStore.CreateFileFromStreamAsync(path, mediaContext.Stream, overwrite);
            _mediaEventHandlers.Invoke((handler, context) => handler.MediaCreated(context), mediaContext, Logger);
            return task;
        }

        public string MapPathToPublicUrl(string path)
        {
            return _cdnBaseUrl + _requestBasePath + "/" + _fileStore.NormalizePath(path);
        }
    }
}
