using Orchard.FileSystem.AppData;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace Orchard.Tests.FileSystem
{
    public class PhysicalAppDataFolderTests : IDisposable
    {
        private string _tempFolder;
        private IAppDataFolder _appDataFolder;

        public class StubAppDataFolderRoot : IAppDataFolderRoot
        {
            public string RootPath { get; set; }
            public string RootFolder { get; set; }
        }

        public static IAppDataFolder CreateAppDataFolder(string tempFolder)
        {
            var folderRoot = new StubAppDataFolderRoot { RootPath = "~/App_Data", RootFolder = tempFolder };
            return new PhysicalAppDataFolder(folderRoot, new StubLoggerFactory());
        }

        public PhysicalAppDataFolderTests()
        {
            _tempFolder = Path.GetTempFileName();
            File.Delete(_tempFolder);
            Directory.CreateDirectory(Path.Combine(_tempFolder, "alpha"));
            File.WriteAllText(Path.Combine(_tempFolder, "alpha\\beta.txt"), "beta-content");
            File.WriteAllText(Path.Combine(_tempFolder, "alpha\\gamma.txt"), "gamma-content");
            Directory.CreateDirectory(Path.Combine(_tempFolder, "alpha\\omega"));

            _appDataFolder = CreateAppDataFolder(_tempFolder);
        }

        public void Dispose()
        {
            Directory.Delete(_tempFolder, true);
        }

        [Fact]
        public void ListFilesShouldContainFileInfo()
        {
            var files = _appDataFolder.ListFiles("alpha").Select(x => x.Name).ToList();
            Assert.Equal(2, files.Count());
            Assert.Contains("beta.txt", files);
            Assert.Contains("gamma.txt", files);
        }

        [Fact]
        public void NonExistantFolderShouldListAsEmptyCollection()
        {
            var files = _appDataFolder.ListFiles("delta");
            Assert.Equal(0, files.Count());
        }

        [Fact]
        public void PhysicalPathAddsToBasePathAndDoesNotNeedToExist()
        {
            var physicalPath = _appDataFolder.MapPath("delta\\epsilon.txt");
            Assert.Equal(Path.Combine(_tempFolder, "delta\\epsilon.txt"), physicalPath);
        }

        [Fact]
        public void ListSubdirectoriesShouldContainFullSubpath()
        {
            var files = _appDataFolder.ListDirectories("alpha").Select(x => x.Name);
            Assert.Equal(1, files.Count());
            Assert.Contains("omega", files);
        }

        [Fact]
        public void ListSubdirectoriesShouldWorkInRoot()
        {
            var files = _appDataFolder.ListDirectories("").Select(x => x.Name);
            Assert.Equal(1, files.Count());
            Assert.Contains("alpha", files);
        }

        [Fact]
        public void NonExistantFolderShouldListDirectoriesAsEmptyCollection()
        {
            var files = _appDataFolder.ListDirectories("delta");
            Assert.Equal(0, files.Count());
        }

        [Fact]
        public void CreateFileWillCauseDirectoryToBeCreated()
        {
            Assert.False(Directory.Exists(Path.Combine(_tempFolder, "alpha\\omega\\foo")));
            _appDataFolder.CreateFile("alpha\\omega\\foo\\bar.txt", "quux");
            Assert.True(Directory.Exists(Path.Combine(_tempFolder, "alpha\\omega\\foo")));
        }


        [Fact]
        public void FilesCanBeReadBack()
        {
            _appDataFolder.CreateFile("alpha\\gamma\\foo\\bar.txt", @"
this is
a
test");
            var text = File.ReadAllText(_appDataFolder.GetFileInfo("alpha\\gamma\\foo\\bar.txt").PhysicalPath);
            Assert.Equal(@"
this is
a
test", text);
        }

        [Fact]
        public void FileExistsReturnsFalseForNonExistingFile()
        {
            Assert.False(_appDataFolder.GetFileInfo("notexisting").Exists);
        }

        [Fact]
        public void FileExistsReturnsTrueForExistingFile()
        {
            _appDataFolder.CreateFile("alpha\\foo\\bar.txt", "");
            Assert.True(_appDataFolder.GetFileInfo("alpha\\foo\\bar.txt").Exists);
        }
    }
}