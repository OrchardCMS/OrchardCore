//using System;
//using System.IO;
//using System.Linq;
//using Orchard.FileSystems.AppData;
//using Orchard.Tests.Stubs;
//using Xunit;

//namespace Orchard.Tests.FileSystems.AppData {
//    public class AppDataFolderTests : IDisposable {
//        private string _tempFolder;
//        private IAppDataFolder _appDataFolder;

//        public class StubAppDataFolderRoot : IAppDataFolderRoot {
//            public string RootPath { get; set; }
//            public string RootFolder { get; set; }
//        }

//        public static IAppDataFolder CreateAppDataFolder(string tempFolder) {
//            var folderRoot = new StubAppDataFolderRoot {RootPath = "~/App_Data", RootFolder = tempFolder};
//            return new AppDataFolder(folderRoot);
//        }

//        public AppDataFolderTests() {
//            _tempFolder = Path.GetTempFileName();
//            File.Delete(_tempFolder);
//            Directory.CreateDirectory(Path.Combine(_tempFolder, "alpha"));
//            File.WriteAllText(Path.Combine(_tempFolder, "alpha\\beta.txt"), "beta-content");
//            File.WriteAllText(Path.Combine(_tempFolder, "alpha\\gamma.txt"), "gamma-content");
//            Directory.CreateDirectory(Path.Combine(_tempFolder, "alpha\\omega"));

//            _appDataFolder = CreateAppDataFolder(_tempFolder);
//        }

//        public void Dispose()
//        {
//            Directory.Delete(_tempFolder, true);
//        }

//        [Fact]
//        public void ListFilesShouldContainSubPathAndFileName() {
//            var files = _appDataFolder.ListFiles("alpha");
//            Assert.Same(files.Count(), 2);
//            Assert.Contains(files, "alpha/beta.txt");
//            Assert.Contains(files,"alpha/gamma.txt");
//        }

//        [Fact]
//        public void NonExistantFolderShouldListAsEmptyCollection() {
//            var files = _appDataFolder.ListFiles("delta");
//            Assert.That(files.Count(), Is.EqualTo(0));
//        }

//        [Fact]
//        public void PhysicalPathAddsToBasePathAndDoesNotNeedToExist() {
//            var physicalPath = _appDataFolder.MapPath("delta\\epsilon.txt");
//            Assert.That(physicalPath, Is.EqualTo(Path.Combine(_tempFolder, "delta\\epsilon.txt")));
//        }

//        [Fact]
//        public void ListSubdirectoriesShouldContainFullSubpath() {
//            var files = _appDataFolder.ListDirectories("alpha");
//            Assert.That(files.Count(), Is.EqualTo(1));
//            Assert.That(files, Has.Some.EqualTo("alpha/omega"));
//        }

//        [Fact]
//        public void ListSubdirectoriesShouldWorkInRoot() {
//            var files = _appDataFolder.ListDirectories("");
//            Assert.That(files.Count(), Is.EqualTo(1));
//            Assert.That(files, Has.Some.EqualTo("alpha"));
//        }


//        [Fact]
//        public void NonExistantFolderShouldListDirectoriesAsEmptyCollection() {
//            var files = _appDataFolder.ListDirectories("delta");
//            Assert.That(files.Count(), Is.EqualTo(0));
//        }

//        [Fact]
//        public void CreateFileWillCauseDirectoryToBeCreated() {
//            Assert.That(Directory.Exists(Path.Combine(_tempFolder, "alpha\\omega\\foo")), Is.False);
//            _appDataFolder.CreateFile("alpha\\omega\\foo\\bar.txt", "quux");
//            Assert.That(Directory.Exists(Path.Combine(_tempFolder, "alpha\\omega\\foo")), Is.True);
//        }


//        [Fact]
//        public void FilesCanBeReadBack() {            
//            _appDataFolder.CreateFile("alpha\\gamma\\foo\\bar.txt", @"
//this is
//a
//test");
//            var text = _appDataFolder.ReadFile("alpha\\gamma\\foo\\bar.txt");
//            Assert.That(text, Is.EqualTo(@"
//this is
//a
//test"));
//        }

//        [Fact]
//        public void FileExistsReturnsFalseForNonExistingFile() {
//            Assert.That(_appDataFolder.FileExists("notexisting"), Is.False);
//        }

//        [Fact]
//        public void FileExistsReturnsTrueForExistingFile() {
//            _appDataFolder.CreateFile("alpha\\foo\\bar.txt", "");
//            Assert.That(_appDataFolder.FileExists("alpha\\foo\\bar.txt"), Is.True);
//        }
//    }
//}
