using Orchard.FileSystem.AppData;
using Microsoft.Extensions.FileProviders;
using Orchard.FileSystem;
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

        public IAppDataFolder CreateAppDataFolder(string tempFolder)
        {
            return new PhysicalAppDataFolder(
                new OrchardFileSystem("App_Data", new PhysicalFileProvider(tempFolder), new NullLogger<OrchardFileSystem>()), 
                new NullLogger<PhysicalAppDataFolder>());
        }

        public PhysicalAppDataFolderTests()
        {
            _tempFolder = Path.GetTempFileName();
            File.Delete(_tempFolder);
            Directory.CreateDirectory(_tempFolder); 
             _appDataFolder = CreateAppDataFolder(_tempFolder);

            string alpha1 = String.Format("App_Data{0}alpha{0}beta.txt",Path.DirectorySeparatorChar);
            string alpha2 = String.Format("App_Data{0}alpha{0}gamma.txt",Path.DirectorySeparatorChar);
            string alpha3 = String.Format("App_Data{0}alpha{0}omega",Path.DirectorySeparatorChar);

            Directory.CreateDirectory(Path.Combine(_tempFolder, String.Format("App_Data{0}alpha",Path.DirectorySeparatorChar)));
            File.WriteAllText(Path.Combine(_tempFolder, alpha1), "beta-content");
            File.WriteAllText(Path.Combine(_tempFolder, alpha2), "gamma-content");
            Directory.CreateDirectory(Path.Combine(_tempFolder, alpha3));
        }

        public void Dispose()
        {
            Directory.Delete(_tempFolder, true);
        }

        [Fact]
        public void ListFilesShouldContainFileInfo()
        {
            var files = _appDataFolder.ListFiles("alpha").Select(x => x.Name).ToList();
            Assert.Equal(3, files.Count());
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
            var physicalPath = _appDataFolder.MapPath(String.Format("delta{0}epsilon.txt",Path.DirectorySeparatorChar));
            Assert.Equal(Path.Combine(_tempFolder, String.Format("App_Data{0}delta{0}epsilon.txt",Path.DirectorySeparatorChar)), physicalPath);
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
            var files = _appDataFolder.ListDirectories("/").Select(x => x.Name);
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
            Assert.False(Directory.Exists(Path.Combine(_tempFolder, String.Format("App_Data{0}alpha{0}omega{0}foo",Path.DirectorySeparatorChar))));
            _appDataFolder.CreateFileAsync(String.Format("alpha{0}omega{0}foo{0}bar.txt",Path.DirectorySeparatorChar),"quux").Wait();
            Assert.True(Directory.Exists(Path.Combine(_tempFolder, String.Format("App_Data{0}alpha{0}omega{0}foo",
                                                                        Path.DirectorySeparatorChar))));
        }


        [Fact]
        public void FilesCanBeReadBack()
        {
            _appDataFolder.CreateFileAsync(String.Format("alpha{0}gamma{0}foo{0}bar.txt",Path.DirectorySeparatorChar), @"
this is
a
test").Wait();
            var text = File.ReadAllText(_appDataFolder.GetFileInfo(String.Format("alpha{0}gamma{0}foo{0}bar.txt",
                                                        Path.DirectorySeparatorChar)).PhysicalPath);
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
            _appDataFolder.CreateFile(String.Format("alpha{0}foo{0}bar.txt",Path.DirectorySeparatorChar));
            Assert.True(_appDataFolder.GetFileInfo(String.Format("alpha{0}foo{0}bar.txt",Path.DirectorySeparatorChar)).Exists);
        }
    }
}