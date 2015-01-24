//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using OrchardVNext.FileSystems.AppData;
//using OrchardVNext.Services;

//namespace OrchardVNext.Tests.Stubs {
//    public class StubAppDataFolder : IAppDataFolder {
//        private readonly StubFileSystem _fileSystem;

//        public StubAppDataFolder() {
//            _fileSystem = new StubFileSystem(_clock);
//        }

//        public StubFileSystem FileSystem {
//            get { return _fileSystem; }
//        }

//        public IEnumerable<string> ListFiles(string path) {
//            var entry = _fileSystem.GetDirectoryEntry(path);
//            if (entry == null)
//                throw new ArgumentException();

//            return entry.Entries.Where(e => e is StubFileSystem.FileEntry).Select(e => Combine(path, e.Name));
//        }

//        public IEnumerable<string> ListDirectories(string path) {
//            var entry = _fileSystem.GetDirectoryEntry(path);
//            if (entry == null)
//                throw new ArgumentException();

//            return entry.Entries.Where(e => e is StubFileSystem.DirectoryEntry).Select(e => Combine(path, e.Name));
//        }

//        public bool FileExists(string path) {
//            return _fileSystem.GetFileEntry(path) != null;
//        }

//        public string Combine(params string[] paths) {
//            return Path.Combine(paths).Replace(Path.DirectorySeparatorChar, '/');
//        }

//        public void CreateFile(string path, string content) {
//            using (var stream = CreateFile(path)) {
//                using (var writer = new StreamWriter(stream)) {
//                    writer.Write(content);
//                }
//            }
//        }

//        public Stream CreateFile(string path) {
//            return _fileSystem.CreateFile(path);
//        }

//        public string ReadFile(string path) {
//            using (var stream = OpenFile(path)) {
//                using (var reader = new StreamReader(stream)) {
//                    return reader.ReadToEnd();
//                }
//            }
//        }

//        public Stream OpenFile(string path) {
//            return _fileSystem.OpenFile(path);
//        }

//        public void StoreFile(string sourceFileName, string destinationPath) {
//            using (var inputStream = File.OpenRead(sourceFileName)) {
//                using (var outputStream = _fileSystem.CreateFile(destinationPath)) {
//                    byte[] buffer = new byte[1024];
//                    for (; ; ) {
//                        var count = inputStream.Read(buffer, 0, buffer.Length);
//                        if (count == 0)
//                            break;
//                        outputStream.Write(buffer, 0, count);
//                    }
//                }
//            }
//        }

//        public void DeleteFile(string path) {
//            _fileSystem.DeleteFile(path);
//        }

//        public DateTime GetFileLastWriteTimeUtc(string path) {
//            var entry = _fileSystem.GetFileEntry(path);
//            if (entry == null)
//                throw new ArgumentException();
//            return entry.LastWriteTimeUtc;
//        }

//        public void CreateDirectory(string path) {
//            _fileSystem.CreateDirectoryEntry(path);
//        }

//        public bool DirectoryExists(string path) {
//            return _fileSystem.GetDirectoryEntry(path) != null;
//        }

//        public string MapPath(string path) {
//            throw new NotImplementedException();
//        }

//        public string GetVirtualPath(string path) {
//            throw new NotImplementedException();
//        }

//        public DateTime GetLastWriteTimeUtc(string path) {
//            var entry = _fileSystem.GetFileEntry(path);
//            if (entry == null)
//                throw new InvalidOperationException();
//            return entry.LastWriteTimeUtc;
//        }
//    }
//}