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
            try
            {
                var physicalPath = GetPhysicalPath(path);

                var fileInfo = new PhysicalFileInfo(new FileInfo(physicalPath));

                if (fileInfo.Exists)
                {
                    return Task.FromResult<IFileStoreEntry>(new FileSystemStoreEntry(path, fileInfo));
                }

                return Task.FromResult<IFileStoreEntry>(null);
            }
            catch (Exception ex)
            {
                throw new FileStoreException($"Cannot get file info with path '{path}'.", ex);
            }
        }

        public Task<IFileStoreEntry> GetDirectoryInfoAsync(string path)
        {
            try
            {
                var physicalPath = GetPhysicalPath(path);

                var directoryInfo = new PhysicalDirectoryInfo(new DirectoryInfo(physicalPath));

                if (directoryInfo.Exists)
                {
                    return Task.FromResult<IFileStoreEntry>(new FileSystemStoreEntry(path, directoryInfo));
                }

                return Task.FromResult<IFileStoreEntry>(null);
            }
            catch (Exception ex)
            {
                throw new FileStoreException($"Cannot get directory info with path '{path}'.", ex);
            }
        }

        public IAsyncEnumerable<IFileStoreEntry> GetDirectoryContentAsync(string path = null, bool includeSubDirectories = false)
        {
            try
            {
                var physicalPath = GetPhysicalPath(path);
                var results = new List<IFileStoreEntry>();

                if (!Directory.Exists(physicalPath))
                {
                    return results.ToAsyncEnumerable();
                }

                // Add directories.
                results.AddRange(
                    Directory
                        .GetDirectories(physicalPath, "*", includeSubDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
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
                        .GetFiles(physicalPath, "*", includeSubDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                        .Select(f =>
                        {
                            var fileSystemInfo = new PhysicalFileInfo(new FileInfo(f));
                            var fileRelativePath = f.Substring(_fileSystemPath.Length);
                            var filePath = this.NormalizePath(fileRelativePath);
                            return new FileSystemStoreEntry(filePath, fileSystemInfo);
                        }));

                return results.ToAsyncEnumerable();
            }
            catch (Exception ex)
            {
                throw new FileStoreException($"Cannot get directory content with path '{path}'.", ex);
            }
        }

        public Task<bool> TryCreateDirectoryAsync(string path)
        {
            try
            {
                var physicalPath = GetPhysicalPath(path);

                if (File.Exists(physicalPath))
                {
                    throw new FileStoreException($"Cannot create directory because the path '{path}' already exists and is a file.");
                }

                if (Directory.Exists(physicalPath))
                {
                    return Task.FromResult(false);
                }

                Directory.CreateDirectory(physicalPath);

                return Task.FromResult(true);
            }
            catch (FileStoreException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FileStoreException($"Cannot create directory '{path}'.", ex);
            }
        }

        public Task<bool> TryDeleteFileAsync(string path)
        {
            try
            {
                var physicalPath = GetPhysicalPath(path);

                if (!File.Exists(physicalPath))
                {
                    return Task.FromResult(false);
                }

                File.Delete(physicalPath);

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                throw new FileStoreException($"Cannot delete file '{path}'.", ex);
            }
        }

        public Task<bool> TryDeleteDirectoryAsync(string path)
        {
            try
            {
                var physicalPath = GetPhysicalPath(path);

                if (!Directory.Exists(physicalPath))
                {
                    return Task.FromResult(false);
                }

                Directory.Delete(physicalPath, recursive: true);

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                throw new FileStoreException($"Cannot delete directory '{path}'.", ex);
            }
        }

        public Task MoveFileAsync(string oldPath, string newPath)
        {
            try
            {
                var physicalOldPath = GetPhysicalPath(oldPath);

                if (!File.Exists(physicalOldPath))
                {
                    throw new FileStoreException($"Cannot move file '{oldPath}' because it does not exist.");
                }

                var physicalNewPath = GetPhysicalPath(newPath);

                if (File.Exists(physicalNewPath) || Directory.Exists(physicalNewPath))
                {
                    throw new FileStoreException($"Cannot move file because the new path '{newPath}' already exists.");
                }

                File.Move(physicalOldPath, physicalNewPath);

                return Task.CompletedTask;
            }
            catch (FileStoreException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FileStoreException($"Cannot move file '{oldPath}' to '{newPath}'.", ex);
            }
        }

        public Task CopyFileAsync(string srcPath, string dstPath)
        {
            try
            {
                var physicalSrcPath = GetPhysicalPath(srcPath);

                if (!File.Exists(physicalSrcPath))
                {
                    throw new FileStoreException($"The file '{srcPath}' does not exist.");
                }

                var physicalDstPath = GetPhysicalPath(dstPath);

                if (File.Exists(physicalDstPath) || Directory.Exists(physicalDstPath))
                {
                    throw new FileStoreException($"Cannot copy file because the destination path '{dstPath}' already exists.");
                }

                File.Copy(GetPhysicalPath(srcPath), GetPhysicalPath(dstPath));

                return Task.CompletedTask;
            }
            catch (FileStoreException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FileStoreException($"Cannot copy file '{srcPath}' to '{dstPath}'.", ex);
            }
        }

        public Task<Stream> GetFileStreamAsync(string path)
        {
            try
            {
                var physicalPath = GetPhysicalPath(path);

                if (!File.Exists(physicalPath))
                {
                    throw new FileStoreException($"Cannot get file stream because the file '{path}' does not exist.");
                }

                var stream = File.OpenRead(physicalPath);

                return Task.FromResult<Stream>(stream);
            }
            catch (FileStoreException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FileStoreException($"Cannot get file stream of the file '{path}'.", ex);
            }
        }

        public Task<Stream> GetFileStreamAsync(IFileStoreEntry fileStoreEntry)
        {
            try
            {
                var physicalPath = GetPhysicalPath(fileStoreEntry.Path);
                if (!File.Exists(physicalPath))
                {
                    throw new FileStoreException($"Cannot get file stream because the file '{fileStoreEntry.Path}' does not exist.");
                }

                var stream = File.OpenRead(physicalPath);

                return Task.FromResult<Stream>(stream);
            }
            catch (FileStoreException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FileStoreException($"Cannot get file stream of the file '{fileStoreEntry.Path}'.", ex);
            }           
        }

        public async Task<string> CreateFileFromStreamAsync(string path, Stream inputStream, bool overwrite = false)
        {
            try
            {
                var physicalPath = GetPhysicalPath(path);

                if (!overwrite && File.Exists(physicalPath))
                {
                    throw new FileStoreException($"Cannot create file '{path}' because it already exists.");
                }

                if (Directory.Exists(physicalPath))
                {
                    throw new FileStoreException($"Cannot create file '{path}' because it already exists as a directory.");
                }

                // Create directory path if it doesn't exist.
                var physicalDirectoryPath = Path.GetDirectoryName(physicalPath);
                Directory.CreateDirectory(physicalDirectoryPath);

                var fileInfo = new FileInfo(physicalPath);
                using (var outputStream = fileInfo.Create())
                {
                    await inputStream.CopyToAsync(outputStream);
                }

                return path;
            }
            catch (FileStoreException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FileStoreException($"Cannot create file '{path}'.", ex);
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
            try
            {
                path = this.NormalizePath(path);

                var physicalPath = String.IsNullOrEmpty(path) ? _fileSystemPath : Path.Combine(_fileSystemPath, path);

                // Verify that the resulting path is inside the root file system path.
                var pathIsAllowed = Path.GetFullPath(physicalPath).StartsWith(_fileSystemPath, StringComparison.OrdinalIgnoreCase);
                if (!pathIsAllowed)
                {
                    throw new FileStoreException($"The path '{path}' resolves to a physical path outside the file system store root.");
                }

                return physicalPath;
            }
            catch (FileStoreException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FileStoreException($"Cannot resolve physical path with the path '{path}'.", ex);
            }
        }
    }
}
