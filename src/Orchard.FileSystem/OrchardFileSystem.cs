using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using Microsoft.Extensions.Logging;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Orchard.FileSystem
{
    public class OrchardFileSystem : IOrchardFileSystem
    {
        private readonly IFileProvider _fileProvider;
        private readonly ILogger _logger;

        public OrchardFileSystem(
            string rootPath,
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

        private void MakeDestinationFileNameAvailable(IFileInfo fileInfo)
        {
            var destinationFileName = fileInfo.PhysicalPath;
            bool isDirectory = Directory.Exists(destinationFileName);
            // Try deleting the destination first
            try
            {
                if (isDirectory)
                    Directory.Delete(destinationFileName);
                else
                    File.Delete(destinationFileName);
            }
            catch
            {
                // We land here if the file is in use, for example. Let's move on.
            }

            if (isDirectory && Directory.Exists(destinationFileName))
            {
                _logger.LogWarning("Could not delete recipe execution folder {0} under \"App_Data\" folder", destinationFileName);
                return;
            }
            // If destination doesn't exist, we are good
            if (!File.Exists(destinationFileName))
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
        /// Combine a set of paths in to a single path
        /// </summary>
        public string Combine(params string[] paths)
        {
            return Path.Combine(paths).Replace(RootPath, string.Empty).Replace(Path.DirectorySeparatorChar, '/').TrimStart('/');
        }

        public async Task CreateFileAsync(string path, string content)
        {
            using (var stream = CreateFile(path))
            {
                using (var tw = new StreamWriter(stream))
                {
                    await tw.WriteAsync(content);
                }
            }
        }

        public Stream CreateFile(string path)
        {
            var fileInfo = _fileProvider.GetFileInfo(path);

            if (!fileInfo.Exists)
            {
                CreateDirectory(Path.GetDirectoryName(path));
            }

            return File.Create(fileInfo.PhysicalPath);
        }

        public async Task<string> ReadFileAsync(string path)
        {
            var file = _fileProvider.GetFileInfo(path);

            if (!file.Exists)
            {
                return null;
            }

            using (var reader = File.OpenText(file.PhysicalPath))
            {
                return await reader.ReadToEndAsync();
            }
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

            var destinationFileName = GetFileInfo(destinationPath);
            MakeDestinationFileNameAvailable(destinationFileName);
            File.Copy(sourceFileName, destinationFileName.PhysicalPath, true);
        }

        public void DeleteFile(string path)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Deleting file \"{0}\" from \"App_Data\" folder", path);
            }

            MakeDestinationFileNameAvailable(GetFileInfo(path));
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
            return new DirectoryInfo(Path.Combine(RootPath, Combine(path)));
        }

        public IEnumerable<IFileInfo> ListFiles(string path)
        {
            var directory = GetDirectoryInfo(path);
            if (!directory.Exists)
            {
                return Enumerable.Empty<IFileInfo>();
            }

            return Directory.EnumerateFiles(directory.FullName)
                .Select(result => GetFileInfo(Combine(result)));
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

            if (!directory.Exists)
            {
                return Enumerable.Empty<DirectoryInfo>();
            }

            return directory.EnumerateDirectories();
        }
    }
}
