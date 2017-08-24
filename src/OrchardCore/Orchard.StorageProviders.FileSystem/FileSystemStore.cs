using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Orchard.StorageProviders.FileSystem
{
    public class FileSystemStore : IFileStore
    {
        private readonly string _localPath;
        private readonly string _requestUrlPrefix;
        private readonly string _pathPrefix;
        private readonly string _publicPathPrefix;

        public FileSystemStore(string localPath, string requestUrlPrefix, string pathPrefix)
        {
            _localPath = localPath;
            _requestUrlPrefix = String.IsNullOrWhiteSpace(requestUrlPrefix) ? "" : "/" + NormalizePath(requestUrlPrefix);
            _pathPrefix = pathPrefix;
            _publicPathPrefix = Combine(_requestUrlPrefix, _pathPrefix);
        }

        public string Combine(params string[] paths)
        {
            var combined = String.Join("/", paths.Select(x => NormalizePath(x).Trim('/')));

            // Preserve the initial '/' if it's present
            if (paths.Length > 0 && paths[0].StartsWith("/"))
            {
                combined = "/" + combined;
            }

            return combined;
        }

        private string NormalizePath(string path)
        {
            return path.Replace('\\', '/').Replace("//", "/").TrimEnd('/');
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
                    var fileSubPath = f.Substring(_localPath.Length);
                    return new FileSystemFile(fileSubPath, _publicPathPrefix, fileInfo);
                }).ToArray()
            );

            // Add files
            results.AddRange(Directory
                .GetFiles(GetPhysicalPath(subpath))
                .Select(f =>
                {
                    var fileInfo = new FileInfo(f);
                    var fileSubPath = f.Substring(_localPath.Length);
                    return new FileSystemFile(fileSubPath, _publicPathPrefix, fileInfo);
                }).ToArray()
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
                // Use CreateSubdirectory to ensure the directory doesn't go over its boundaries
                new DirectoryInfo(_localPath).CreateSubdirectory(subpath);
                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        public Task<IFile> MapFileAsync(string absoluteUrl)
        {
            if (!absoluteUrl.StartsWith(_pathPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(default(IFile));
            }

            return GetFileAsync(absoluteUrl.Substring(_pathPrefix.Length));
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

        public async Task<bool> TrySaveStreamAsync(string filename, Stream inputStream)
        {
            try
            {
                var path = Path.GetDirectoryName(filename);
                var mediaFolder = await GetFolderAsync(path);

                if (mediaFolder == null)
                {
                    await TryCreateFolderAsync(path);
                }

                var fileInfo = new FileInfo(GetPhysicalPath(filename));

                // Ensure the file will be in the targetted folder
                var directoryInfo = fileInfo.Directory;
                var rootDirectory = new DirectoryInfo(_localPath);
                if (!directoryInfo.FullName.StartsWith(rootDirectory.FullName))
                {
                    throw new ArgumentException("Attemp to create a file outside of the Media folder: " + filename);
                }

                using (var outputStream = fileInfo.Create())
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
            subpath = "/" + NormalizePath(subpath);

            string physicalPath = string.IsNullOrEmpty(subpath) ? _localPath : _localPath + subpath;
            return ValidatePath(_localPath, physicalPath);
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
            return Combine(_publicPathPrefix, subpath);
        }
    }
}
