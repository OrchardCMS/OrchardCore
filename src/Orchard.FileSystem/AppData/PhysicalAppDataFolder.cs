using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNet.FileProviders;
using Orchard.Localization;

namespace Orchard.FileSystem.AppData
{
    public class PhysicalAppDataFolder : IAppDataFolder
    {
        private readonly IFileProvider _fileProvider;
        private readonly ILogger _logger;

        public PhysicalAppDataFolder(IAppDataFolderRoot root,
            ILoggerFactory loggerFactory)
        {
            if (!Directory.Exists(root.RootFolder))
                Directory.CreateDirectory(root.RootFolder);

            _fileProvider = new PhysicalFileProvider(root.RootFolder);
            _logger = loggerFactory.CreateLogger<PhysicalAppDataFolder>();

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        private void MakeDestinationFileNameAvailable(string destinationFileName)
        {
            var directory = GetDirectoryInfo(destinationFileName);
            // Try deleting the destination first
            try
            {
                if (directory.IsDirectory)
                    Directory.Delete(destinationFileName);
                else
                    File.Delete(destinationFileName);
            }
            catch
            {
                // We land here if the file is in use, for example. Let's move on.
            }

            if (directory.IsDirectory && GetDirectoryInfo(destinationFileName).Exists)
            {
                _logger.LogWarning("Could not delete recipe execution folder {0} under \"App_Data\" folder", destinationFileName);
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
        /// Combine a set of virtual paths into a virtual path relative to "~/App_Data"
        /// </summary>
        public string Combine(params string[] paths)
        {
            return Path.Combine(paths).Replace(Path.DirectorySeparatorChar, '/');
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
                Directory.CreateDirectory(Path.GetDirectoryName(fileInfo.PhysicalPath));
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
            _logger.LogInformation("Storing file \"{0}\" as \"{1}\" in \"App_Data\" folder", sourceFileName, destinationPath);

            var destinationFileName = GetFileInfo(destinationPath).PhysicalPath;
            MakeDestinationFileNameAvailable(destinationFileName);
            File.Copy(sourceFileName, destinationFileName, true);
        }

        public void DeleteFile(string path)
        {
            _logger.LogInformation("Deleting file \"{0}\" from \"App_Data\" folder", path);
            MakeDestinationFileNameAvailable(GetFileInfo(path).PhysicalPath);
        }

        public void CreateDirectory(string path)
        {
            Directory.CreateDirectory(GetFileInfo(path).PhysicalPath);
        }

        public bool DirectoryExists(string path)
        {
            return GetFileInfo(path).Exists;
        }

        public DateTime GetFileLastWriteTimeUtc(string path)
        {
            return File.GetLastWriteTimeUtc(GetFileInfo(path).PhysicalPath);
        }

        public IFileInfo GetFileInfo(string path)
        {
            return _fileProvider.GetFileInfo(path);
        }

        public IFileInfo GetDirectoryInfo(string path)
        {
            return _fileProvider.GetFileInfo(path);
        }

        public IEnumerable<IFileInfo> ListFiles(string path)
        {
            var directoryContents = _fileProvider.GetDirectoryContents(path);
            if (!directoryContents.Exists)
                return Enumerable.Empty<IFileInfo>();

            return directoryContents
                .Where(x => !x.IsDirectory);
        }

        public IEnumerable<IFileInfo> ListDirectories(string path)
        {
            var directoryContents = _fileProvider.GetDirectoryContents(path);
            if (!directoryContents.Exists)
                return Enumerable.Empty<IFileInfo>();

            return directoryContents
                .Where(x => x.IsDirectory);
        }

        public string MapPath(string path)
        {
            return _fileProvider.GetFileInfo(path).PhysicalPath;
        }
    }
}