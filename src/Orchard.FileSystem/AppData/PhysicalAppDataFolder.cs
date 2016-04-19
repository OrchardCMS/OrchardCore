using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;

namespace Orchard.FileSystem.AppData
{
    public class PhysicalAppDataFolder : IAppDataFolder
    {
        private readonly IOrchardFileSystem _fileSystem;
        private readonly ILogger _logger;

        private static string InternalRootPath = "App_Data";

        public PhysicalAppDataFolder(
            IOrchardFileSystem parentFileSystem,
            ILogger<PhysicalAppDataFolder> logger)
        {
            _logger = logger;

            if (!parentFileSystem.DirectoryExists(InternalRootPath))
            {
                parentFileSystem.CreateDirectory(InternalRootPath);
            }

            var root = parentFileSystem.GetDirectoryInfo(InternalRootPath).FullName;

            _fileSystem = new OrchardFileSystem(root, new PhysicalFileProvider(root), _logger);
        }

        public string RootPath
        {
            get
            {
                return _fileSystem.RootPath;
            }
        }

        /// <summary>
        /// Combine a set of virtual paths into a virtual path relative to "~/App_Data"
        /// </summary>
        public string Combine(params string[] paths)
        {
            return _fileSystem.Combine(paths);
        }

        public void CreateFile(string path, string content)
        {
            _fileSystem.CreateFile(path, content);
        }

        public Stream CreateFile(string path)
        {
            return _fileSystem.CreateFile(path);
        }

        public string ReadFile(string path)
        {
            return _fileSystem.ReadFile(path);
        }

        public Stream OpenFile(string path)
        {
            return _fileSystem.OpenFile(path);
        }

        public void StoreFile(string sourceFileName, string destinationPath)
        {
            _fileSystem.StoreFile(sourceFileName, destinationPath);
        }

        public void DeleteFile(string path)
        {
            _fileSystem.DeleteFile(path);
        }

        public void CreateDirectory(string path)
        {
            _fileSystem.CreateDirectory(path);
        }

        public bool DirectoryExists(string path)
        {
            return _fileSystem.DirectoryExists(path);
        }

        public DateTimeOffset GetFileLastWriteTimeUtc(string path)
        {
            return _fileSystem.GetFileLastWriteTimeUtc(path);
        }

        public IFileInfo GetFileInfo(string path)
        {
            return _fileSystem.GetFileInfo(path);
        }

        public DirectoryInfo GetDirectoryInfo(string path)
        {
            return _fileSystem.GetDirectoryInfo(path);
        }

        public IEnumerable<IFileInfo> ListFiles(string path)
        {
            return _fileSystem.ListFiles(path);
        }

        public IEnumerable<DirectoryInfo> ListDirectories(string path)
        {
            return _fileSystem.ListDirectories(path);
        }

        public string MapPath(string path)
        {
            return _fileSystem.GetFileInfo(path).PhysicalPath;
        }
    }
}