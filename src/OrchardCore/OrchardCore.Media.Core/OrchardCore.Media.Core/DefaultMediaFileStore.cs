using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private readonly IEnumerable<IMediaCreatingEventHandler> _mediaCreatingEventHandlers;
        private readonly ILogger _logger;

        public DefaultMediaFileStore(
            IFileStore fileStore,
            string requestBasePath,
            string cdnBaseUrl,
            IEnumerable<IMediaEventHandler> mediaEventHandlers,
            IEnumerable<IMediaCreatingEventHandler> mediaCreatingEventHandlers,
            ILogger<DefaultMediaFileStore> logger
            )
        {
            _fileStore = fileStore;

            // Ensure trailing slash removed.
            _requestBasePath = requestBasePath.TrimEnd('/');

            // Media options configuration ensures any trailing slash is removed.
            _cdnBaseUrl = cdnBaseUrl;

            _mediaEventHandlers = mediaEventHandlers;
            _mediaCreatingEventHandlers = mediaCreatingEventHandlers;
            _logger = logger;
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

        public async Task<bool> TryDeleteFileAsync(string path)
        {
            var deletingContext = new MediaDeletingContext
            {
                Path = path
            };

            await _mediaEventHandlers.InvokeAsync((handler, context) => handler.MediaDeletingFileAsync(context), deletingContext, _logger);

            var result = await _fileStore.TryDeleteFileAsync(deletingContext.Path);

            var deletedContext = new MediaDeletedContext
            {
                Path = path,
                Result = result
            };

            await _mediaEventHandlers.InvokeAsync((handler, deletedContext) => handler.MediaDeletedFileAsync(deletedContext), deletedContext, _logger);

            return result;
        }

        public async Task<bool> TryDeleteDirectoryAsync(string path)
        {
            var deletingContext = new MediaDeletingContext
            {
                Path = path
            };

            await _mediaEventHandlers.InvokeAsync((handler, context) => handler.MediaDeletingDirectoryAsync(context), deletingContext, _logger);

            var result = await _fileStore.TryDeleteDirectoryAsync(path);

            var deletedContext = new MediaDeletedContext
            {
                Path = path,
                Result = result
            };

            await _mediaEventHandlers.InvokeAsync((handler, deletedContext) => handler.MediaDeletedDirectoryAsync(deletedContext), deletedContext, _logger);

            return result;
        }

        public async Task MoveFileAsync(string oldPath, string newPath)
        {
            var context = new MediaMoveContext
            {
                OldPath = oldPath,
                NewPath = newPath
            };

            await _mediaEventHandlers.InvokeAsync((handler, context) => handler.MediaMovingAsync(context), context, _logger);

            await _fileStore.MoveFileAsync(context.OldPath, context.NewPath);

            await _mediaEventHandlers.InvokeAsync((handler, context) => handler.MediaMovedAsync(context), context, _logger);
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

        public async Task<string> CreateFileFromStreamAsync(string path, Stream inputStream, bool overwrite = false)
        {
            if (_mediaCreatingEventHandlers.Any())
            {
                // Follows https://rules.sonarsource.com/csharp/RSPEC-3966
                // Assumes that each stream should be disposed of only once by it's caller.
                var outputStream = inputStream;
                try
                {
                    var context = new MediaCreatingContext
                    {
                        Path = path
                    };

                    foreach (var mediaCreatingEventHandler in _mediaCreatingEventHandlers)
                    {
                        // Creating stream disposed by using.
                        using (var creatingStream = outputStream)
                        {
                            // Stop disposal of inputStream, as creating stream is the object to dispose.
                            inputStream = null;
                            // Outputstream must be created by event handler.
                            outputStream = null;

                            outputStream = await mediaCreatingEventHandler.MediaCreatingAsync(context, creatingStream);
                        }
                    }

                    return await _fileStore.CreateFileFromStreamAsync(context.Path, outputStream, overwrite);
                }
                finally
                {
                    // This disposes the last outputStream.
                    outputStream?.Dispose();
                }
            }
            else
            {
                return await _fileStore.CreateFileFromStreamAsync(path, inputStream, overwrite);
            }
        }

        public string MapPathToPublicUrl(string path)
        {
            return _cdnBaseUrl + _requestBasePath + "/" + _fileStore.NormalizePath(path);
        }
    }
}
