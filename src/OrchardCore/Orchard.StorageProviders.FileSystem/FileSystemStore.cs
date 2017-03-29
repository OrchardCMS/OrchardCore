using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Orchard.StorageProviders.FileSystem
{
    public class FileSystemStore : IFileStore
    {
        private readonly string _localPathPrefix;
        private readonly string _publicPathPrefix;

        public string LocalBasePath => _localPathPrefix;

        public FileSystemStore(string localPathPrefix, string publicPathPrefix)
        {
            _localPathPrefix = localPathPrefix;
            _publicPathPrefix = publicPathPrefix;
        }

        public string Combine(params string[] paths)
        {
            return Path.Combine(paths);
        }

        public Task<bool> TryCopyFileAsync(string originalPath, string duplicatePath)
        {
            try
            {
                File.Copy(GetPhysicalPath(originalPath), GetPhysicalPath(duplicatePath));
                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }            
        }

        public Task<bool> TryDeleteFileAsync(string subpath)
        {
            try
            {
                File.Delete(GetPhysicalPath(subpath));
                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        public Task<bool> TryDeleteFolderAsync(string subpath)
        {
            try
            {
                Directory.Delete(GetPhysicalPath(subpath), true);
                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        public Task<IEnumerable<IFile>> GetDirectoryContentAsync(string subpath = null)
        {
            var results = new List<IFile>();

            // Add directories
            results.AddRange(Directory
                .GetDirectories(GetPhysicalPath(subpath))
                .Select(f =>
                {
                    var fileInfo = new DirectoryInfo(f);
                    var fileSubPath = f.Substring(_localPathPrefix.Length + 1);
                    return new FileSystemFile(fileSubPath, _publicPathPrefix, fileInfo);
                })
            );

            // Add files
            results.AddRange(Directory
                .GetFiles(GetPhysicalPath(subpath))
                .Select(f =>
                {
                    var fileInfo = new FileInfo(f);
                    var fileSubPath = f.Substring(_localPathPrefix.Length + 1);
                    return new FileSystemFile(fileSubPath, _publicPathPrefix, fileInfo);
                })
            );

            return Task.FromResult((IEnumerable<IFile>)results);
        }

        public Task<IFile> GetFileAsync(string subpath)
        {
            var physicalPath = GetPhysicalPath(subpath);

            var fileInfo = new FileInfo(physicalPath);

            if (fileInfo.Exists)
            {
                return Task.FromResult<IFile>(new FileSystemFile(subpath, _publicPathPrefix, fileInfo));
            }

            return Task.FromResult<IFile>(null);
        }

        public Task<IFile> GetFolderAsync(string subpath)
        {
            var physicalPath = GetPhysicalPath(subpath);

            var directoryInfo = new DirectoryInfo(physicalPath);

            if (directoryInfo.Exists)
            {
                return Task.FromResult<IFile>(new FileSystemFile(subpath, _publicPathPrefix, directoryInfo));
            }

            return Task.FromResult<IFile>(null);
        }

        public Task<bool> TryCreateFolderAsync(string subpath)
        {
            try
            {
                Directory.CreateDirectory(GetPhysicalPath(subpath));
                return Task.FromResult(true); ;
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        public Task<IFile> MapFileAsync(string absoluteUrl)
        {
            if (!absoluteUrl.StartsWith(_publicPathPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return GetFileAsync(GetPhysicalPath(absoluteUrl.Substring(_publicPathPrefix.Length)));
        }

        public Task<bool> TryMoveFileAsync(string oldPath, string newPath)
        {
            try
            {
                File.Move(GetPhysicalPath(oldPath), GetPhysicalPath(newPath));
                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(true);
            }
        }

        public Task<bool> TryMoveFolderAsync(string oldPath, string newPath)
        {
            try
            {
                Directory.Move(GetPhysicalPath(oldPath), GetPhysicalPath(newPath));
                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        public async Task<bool> TrySaveStreamAsync(string subpath, Stream inputStream)
        {
            try
            {
                using (var outputStream = File.Create(GetPhysicalPath(subpath)))
                {
                    await inputStream.CopyToAsync(outputStream);
                }
            }
            catch
            {
                return false;
            }

            return true;
        }
        
        private string GetPhysicalPath(string subpath)
        {
            string physicalPath = string.IsNullOrEmpty(subpath) ? _localPathPrefix : Path.Combine(_localPathPrefix, subpath);
            return ValidatePath(_localPathPrefix, physicalPath);
        }

        /// <summary>
        /// Determines if a path lies within the base path boundaries.
        /// If not, an exception is thrown.
        /// </summary>
        /// <param name="basePath">The base path which boundaries are not to be transposed.</param>
        /// <param name="physicalPath">The path to determine.</param>
        /// <rereturns>The mapped path if valid.</rereturns>
        /// <exception cref="ArgumentException">If the path is invalid.</exception>
        public static string ValidatePath(string basePath, string physicalPath)
        {
            // Check that we are indeed within the storage directory boundaries
            var valid = Path.GetFullPath(physicalPath).StartsWith(Path.GetFullPath(basePath), StringComparison.OrdinalIgnoreCase);
            
            if (!valid)
            {
                throw new ArgumentException("Invalid path");
            }

            return physicalPath;
        }

        public string GetPublicUrl(string subpath)
        {
            return Combine(_publicPathPrefix, subpath).Replace('\\', '/');
        }
    }
}
