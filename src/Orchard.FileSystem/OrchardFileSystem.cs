using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using Microsoft.Extensions.Logging;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Orchard.FileSystem
{
    public class OrchardFileSystem : IOrchardFileSystem
    {
        private readonly IFileProvider _fileProvider;
        private readonly ILogger _logger;

        public OrchardFileSystem(string rootPath,
            IFileProvider fileProvider,
            ILogger logger)
        {
            _fileProvider = fileProvider;
            _logger = logger;

            RootPath = rootPath;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public string RootPath
        {
            get; private set;
        }

        private void MakeDestinationFileNameAvailable(string destinationFileName)
        {
            var directory = GetDirectoryInfo(destinationFileName);
            // Try deleting the destination first
            try
            {
                if (directory.Exists)
                {
                    directory.Delete();
                }
            }
            catch
            {
                // We land here if the file is in use, for example. Let's move on.
            }

            if (GetDirectoryInfo(destinationFileName).Exists)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogWarning("Could not delete recipe execution folder {0} under \"App_Data\" folder", destinationFileName);
                }
                return;
            }
            // If destination doesn't exist, we are good
            if (!GetFileInfo(destinationFileName).Exists)
                return;

            // Try renaming destination to a unique filename
            const string extension = "deleted";
            for (int i = 0; i < 100; i++)
            {
                var newExtension = (i == 0 ? extension : string.Format("{0}{1}", extension, i));
                var newFileName = Path.ChangeExtension(destinationFileName, newExtension);
                try
                {
                    File.Delete(newFileName);
                    File.Move(destinationFileName, newFileName);

                    // If successful, we are done...
                    return;
                }
                catch
                {
                    // We need to try with another extension
                }
            }

            // Try again with the original filename. This should throw the same exception
            // we got at the very beginning.
            try
            {
                File.Delete(destinationFileName);
            }
            catch (Exception ex)
            {
                if (ex.IsFatal())
                {
                    throw;
                }
                throw new OrchardCoreException(T("Unable to make room for file \"{0}\" in \"App_Data\" folder", destinationFileName), ex);
            }
        }

        /// <summary>
        /// Combine a set of paths in to a signle path
        /// </summary>
        public string Combine(params string[] paths)
        {
            return Path.Combine(paths).Replace(RootPath, string.Empty).Replace(Path.DirectorySeparatorChar, '/').TrimStart('/');
        }

        public void CreateFile(string path, string content)
        {
            using (var stream = CreateFile(path))
            {
                using (var tw = new StreamWriter(stream))
                {
                    tw.Write(content);
                }
            }
        }

        public Stream CreateFile(string path)
        {
            var fileInfo = _fileProvider.GetFileInfo(path);

            if (!fileInfo.Exists)
            {
                CreateDirectory(Path.GetDirectoryName(fileInfo.PhysicalPath));
            }

            return File.Create(fileInfo.PhysicalPath);
        }

        public string ReadFile(string path)
        {
            var file = _fileProvider.GetFileInfo(path);
            return file.Exists ? File.ReadAllText(file.PhysicalPath) : null;
        }

        public Stream OpenFile(string path)
        {
            return _fileProvider.GetFileInfo(path).CreateReadStream();
        }

        public void StoreFile(string sourceFileName, string destinationPath)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Storing file \"{0}\" as \"{1}\" in \"App_Data\" folder", sourceFileName, destinationPath);
            }

            var destinationFileName = GetFileInfo(destinationPath).PhysicalPath;
            MakeDestinationFileNameAvailable(destinationFileName);
            File.Copy(sourceFileName, destinationFileName, true);
        }

        public void DeleteFile(string path)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Deleting file \"{0}\" from \"App_Data\" folder", path);
            }

            MakeDestinationFileNameAvailable(GetFileInfo(path).PhysicalPath);
        }

        public void CreateDirectory(string path)
        {
            GetDirectoryInfo(path).Create();
        }

        public bool DirectoryExists(string path)
        {
            return GetDirectoryInfo(path).Exists;
        }

        public DateTimeOffset GetFileLastWriteTimeUtc(string path)
        {
            return GetFileInfo(path).LastModified;
        }

        public IFileInfo GetFileInfo(string path)
        {
            return _fileProvider.GetFileInfo(path);
        }

        public DirectoryInfo GetDirectoryInfo(string path)
        {
            var physicalPath = _fileProvider.GetFileInfo(path).PhysicalPath;
            if (string.IsNullOrEmpty(physicalPath))
            {
                return null;
            }

            return new DirectoryInfo(physicalPath);
        }

        public IEnumerable<IFileInfo> ListFiles(string path)
        {
            return ListFiles(path, new Matcher());
        }

        public IEnumerable<IFileInfo> ListFiles(string path, Matcher matcher)
        {
            var directory = GetDirectoryInfo(path);
            if (!directory.Exists)
            {
                return Enumerable.Empty<IFileInfo>();
            }

            return matcher.Execute(new DirectoryInfoWrapper(directory))
                    .Files
                    .Select(result => GetFileInfo(Combine(directory.FullName, result.Path)));
        }

        public IEnumerable<DirectoryInfo> ListDirectories(string path)
        {
            var directory = GetDirectoryInfo(path);

            if (directory == null || !directory.Exists)
            {
                return Enumerable.Empty<DirectoryInfo>();
            }

            return directory.EnumerateDirectories();
        }
    }
}
