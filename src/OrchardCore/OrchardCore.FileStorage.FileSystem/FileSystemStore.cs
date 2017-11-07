using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders.Physical;

namespace OrchardCore.FileStorage.FileSystem
{
    public class FileSystemStore : IFileStore
    {
        private readonly string _fileSystemPath;

        public FileSystemStore(string fileSystemPath)
        {
            _fileSystemPath = Path.GetFullPath(fileSystemPath);
        }

        public Task<IFileStoreEntry> GetFileInfoAsync(string path)
        {
            var physicalPath = GetPhysicalPath(path);
            
            var fileInfo = new PhysicalFileInfo(new FileInfo(physicalPath));

            if (fileInfo.Exists)
                return Task.FromResult<IFileStoreEntry>(new FileSystemStoreEntry(path, fileInfo));

            return Task.FromResult<IFileStoreEntry>(null);
        }

        public Task<IFileStoreEntry> GetDirectoryInfoAsync(string path)
        {
            var physicalPath = GetPhysicalPath(path);

            var directoryInfo = new PhysicalDirectoryInfo(new DirectoryInfo(physicalPath));

            if (directoryInfo.Exists)
                return Task.FromResult<IFileStoreEntry>(new FileSystemStoreEntry(path, directoryInfo));

            return Task.FromResult<IFileStoreEntry>(null);
        }

        public Task<IEnumerable<IFileStoreEntry>> GetDirectoryContentAsync(string path = null)
        {
            var physicalPath = GetPhysicalPath(path);

            if (!Directory.Exists(physicalPath))
                throw new FileStoreException($"Cannot get content of directory '{path}' because it does not exist.");

            var results = new List<IFileStoreEntry>();

            // Add directories.
            results.AddRange(
                Directory
                    .GetDirectories(physicalPath)
                    .Select(f =>
                    {
                        var fileSystemInfo = new PhysicalDirectoryInfo(new DirectoryInfo(f));
                        var fileRelativePath = f.Substring(_fileSystemPath.Length);
                        var filePath = this.NormalizePath(fileRelativePath);
                        return new FileSystemStoreEntry(filePath, fileSystemInfo);
                    }));

            // Add files.
            results.AddRange(
                Directory
                    .GetFiles(physicalPath)
                    .Select(f =>
                    {
                        var fileSystemInfo = new PhysicalFileInfo(new FileInfo(f));
                        var fileRelativePath = f.Substring(_fileSystemPath.Length);
                        var filePath = this.NormalizePath(fileRelativePath);
                        return new FileSystemStoreEntry(filePath, fileSystemInfo);
                    }));

            return Task.FromResult((IEnumerable<IFileStoreEntry>)results);
        }

        public Task<bool> TryCreateDirectoryAsync(string path)
        {
            var physicalPath = GetPhysicalPath(path);

            if (File.Exists(physicalPath))
                throw new FileStoreException($"Cannot create directory because the path '{path}' already exists and is a file.");

            if (Directory.Exists(physicalPath))
                return Task.FromResult(false);

            Directory.CreateDirectory(physicalPath);

            return Task.FromResult(true);
        }

        public Task<bool> TryDeleteFileAsync(string path)
        {
            var physicalPath = GetPhysicalPath(path);

            if (!File.Exists(physicalPath))
                return Task.FromResult(false);

            File.Delete(physicalPath);

            return Task.FromResult(true);
        }

        public Task<bool> TryDeleteDirectoryAsync(string path)
        {
            var physicalPath = GetPhysicalPath(path);

            if (!Directory.Exists(physicalPath))
                return Task.FromResult(false);

            Directory.Delete(physicalPath, recursive: true);

            return Task.FromResult(true);
        }

        public Task MoveFileAsync(string oldPath, string newPath)
        {
            var physicalOldPath = GetPhysicalPath(oldPath);

            if (!File.Exists(physicalOldPath))
                throw new FileStoreException($"Cannot move file '{oldPath}' because it does not exist.");

            var physicalNewPath = GetPhysicalPath(newPath);

            if (File.Exists(physicalNewPath) || Directory.Exists(physicalNewPath))
                throw new FileStoreException($"Cannot move file because the new path '{newPath}' already exists.");

            File.Move(physicalOldPath, physicalNewPath);

            return Task.CompletedTask;
        }

        public Task CopyFileAsync(string srcPath, string dstPath)
        {
            var physicalSrcPath = GetPhysicalPath(srcPath);

            if (!File.Exists(physicalSrcPath))
                throw new FileStoreException($"The file '{srcPath}' does not exist.");

            var physicalDstPath = GetPhysicalPath(dstPath);

            if (File.Exists(physicalDstPath) || Directory.Exists(physicalDstPath))
                throw new FileStoreException($"Cannot copy file because the destination path '{dstPath}' already exists.");

            File.Copy(GetPhysicalPath(srcPath), GetPhysicalPath(dstPath));

            return Task.CompletedTask;
        }

        public Task<Stream> GetFileStreamAsync(string path)
        {
            var physicalPath = GetPhysicalPath(path);

            if (!File.Exists(physicalPath))
                throw new FileStoreException($"Cannot get file stream because the file '{path}' does not exist.");

            var stream = File.OpenRead(physicalPath);

            return Task.FromResult<Stream>(stream);
        }

        public async Task CreateFileFromStream(string path, Stream inputStream, bool overwrite = false)
        {
            var physicalPath = GetPhysicalPath(path);

            if (!overwrite && File.Exists(physicalPath))
                throw new FileStoreException($"Cannot create file '{path}' because it already exists.");

            if (Directory.Exists(physicalPath))
                throw new FileStoreException($"Cannot create file '{path}' because it already exists as a directory.");

            // Create directory path if it doesn't exist.
            var physicalDirectoryPath = Path.GetDirectoryName(physicalPath);
            Directory.CreateDirectory(physicalDirectoryPath);

            var fileInfo = new FileInfo(physicalPath);
            using (var outputStream = fileInfo.Create())
            {
                await inputStream.CopyToAsync(outputStream);
            }
        }

        /// <summary>
        /// Translates a relative path in the virtual file store to a physical path in the underlying file system.
        /// </summary>
        /// <param name="path">The relative path within the file store.</param>
        /// <returns></returns>
        /// <remarks>The resulting physical path is verified to be inside designated root file system path.</remarks>
        private string GetPhysicalPath(string path)
        {
            path = this.NormalizePath(path);

            var physicalPath = String.IsNullOrEmpty(path) ? _fileSystemPath : Path.Combine(_fileSystemPath, path);
            
            // Verify that the resulting path is inside the root file system path.
            var pathIsAllowed = Path.GetFullPath(physicalPath).StartsWith(_fileSystemPath, StringComparison.OrdinalIgnoreCase);
            if (!pathIsAllowed)
                throw new FileStoreException($"The path '{path}' resolves to a physical path outside the file system store root.");

            return physicalPath;
        }
    }
}
