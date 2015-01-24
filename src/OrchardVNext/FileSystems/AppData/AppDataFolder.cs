using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using OrchardVNext.Localization;
using OrchardVNext.Validation;

namespace OrchardVNext.FileSystems.AppData {
    public class AppDataFolder : IAppDataFolder {
        private readonly IAppDataFolderRoot _root;

        public AppDataFolder(IAppDataFolderRoot root) {
            _root = root;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public string RootFolder => _root.RootFolder;

        public string AppDataPath => _root.RootPath;

        private void MakeDestinationFileNameAvailable(string destinationFileName) {
            bool isDirectory = Directory.Exists(destinationFileName);
            // Try deleting the destination first
            try {
                if (isDirectory)
                    Directory.Delete(destinationFileName);
                else 
                    File.Delete(destinationFileName);
            }
            catch {
                // We land here if the file is in use, for example. Let's move on.
            }

            if (isDirectory && Directory.Exists(destinationFileName)) {
                Logger.Warning("Could not delete recipe execution folder {0} under \"App_Data\" folder", destinationFileName);
                return;
            }
            // If destination doesn't exist, we are good
            if (!File.Exists(destinationFileName))
                return;

            // Try renaming destination to a unique filename
            const string extension = "deleted";
            for (int i = 0; i < 100; i++) {
                var newExtension = (i == 0 ? extension : string.Format("{0}{1}", extension, i));
                var newFileName = Path.ChangeExtension(destinationFileName, newExtension);
                try {
                    File.Delete(newFileName);
                    File.Move(destinationFileName, newFileName);

                    // If successful, we are done...
                    return;
                }
                catch {
                    // We need to try with another extension
                }
            }

            // Try again with the original filename. This should throw the same exception
            // we got at the very beginning.
            try {
                File.Delete(destinationFileName);
            }
            catch (Exception e) {
                throw new OrchardCoreException(T("Unable to make room for file \"{0}\" in \"App_Data\" folder", destinationFileName), e);
            }
        }

        /// <summary>
        /// Combine a set of virtual paths relative to "~/App_Data" into an absolute physical path
        /// starting with "_basePath".
        /// </summary>
        private string CombineToPhysicalPath(params string[] paths) {
            return PathValidation.ValidatePath(RootFolder, Path.Combine(RootFolder, Path.Combine(paths)).Replace('/', Path.DirectorySeparatorChar));
        }

        /// <summary>
        /// Combine a set of virtual paths into a virtual path relative to "~/App_Data"
        /// </summary>
        public string Combine(params string[] paths) {
            return Path.Combine(paths).Replace(Path.DirectorySeparatorChar, '/');
        }

        public string GetVirtualPath(string path) {
            return Combine(AppDataPath, path);
        }

        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public void CreateFile(string path, string content) {
            using (var stream = CreateFile(path)) {
                using (var tw = new StreamWriter(stream)) {
                    tw.Write(content);
                }
            }
        }

        public Stream CreateFile(string path) {
            var filePath = CombineToPhysicalPath(path);
            var folderPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            return File.Create(filePath);
        }

        public string ReadFile(string path) {
            var physicalPath = CombineToPhysicalPath(path);
            return File.Exists(physicalPath) ? File.ReadAllText(physicalPath) : null;
        }

        public Stream OpenFile(string path) {
            return File.OpenRead(CombineToPhysicalPath(path));
        }

        public void StoreFile(string sourceFileName, string destinationPath) {
            Logger.Information("Storing file \"{0}\" as \"{1}\" in \"App_Data\" folder", sourceFileName, destinationPath);

            var destinationFileName = CombineToPhysicalPath(destinationPath);
            MakeDestinationFileNameAvailable(destinationFileName);
            File.Copy(sourceFileName, destinationFileName, true);
        }

        public void DeleteFile(string path) {
            Logger.Information("Deleting file \"{0}\" from \"App_Data\" folder", path);
            MakeDestinationFileNameAvailable(CombineToPhysicalPath(path));
        }

        public DateTime GetFileLastWriteTimeUtc(string path) {
            return File.GetLastWriteTimeUtc(CombineToPhysicalPath(path));
        }

        public bool FileExists(string path) {
            return File.Exists(CombineToPhysicalPath(path));
        }

        public bool DirectoryExists(string path) {
            return Directory.Exists(CombineToPhysicalPath(path));
        }

        public IEnumerable<string> ListFiles(string path) {
            var directoryPath = CombineToPhysicalPath(path);
            if (!Directory.Exists(directoryPath))
                return Enumerable.Empty<string>();

            var files = Directory.GetFiles(directoryPath);

            return files.Select(file => {
                var fileName = Path.GetFileName(file);
                return Combine(path, fileName);
            });
        }

        public IEnumerable<string> ListDirectories(string path) {
            var directoryPath = CombineToPhysicalPath(path);
            if (!Directory.Exists(directoryPath))
                return Enumerable.Empty<string>();

            var files = Directory.GetDirectories(directoryPath);

            return files.Select(file => {
                var fileName = Path.GetFileName(file);
                return Combine(path, fileName);
            });
        }

        public void CreateDirectory(string path) {
            Directory.CreateDirectory(CombineToPhysicalPath(path));
        }

        public string MapPath(string path) {
            return CombineToPhysicalPath(path);
        }
    }
}