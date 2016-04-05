using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileSystemGlobbing;
using Orchard.FileSystem;
using System;
using System.Collections.Generic;
using System.IO;

namespace Orchard.Tests.Stubs
{

    public class StubHostingFileSystem : IOrchardFileSystem
    {
        public string RootPath
        {
            get;
            internal set;
        }

        public string Combine(params string[] paths)
        {
            throw new NotImplementedException();
        }

        public void CreateDirectory(string path)
        {
            throw new NotImplementedException();
        }

        public Stream CreateFile(string path)
        {
            throw new NotImplementedException();
        }

        public void CreateFile(string path, string content)
        {
            throw new NotImplementedException();
        }

        public void DeleteFile(string path)
        {
            throw new NotImplementedException();
        }

        public bool DirectoryExists(string path)
        {
            throw new NotImplementedException();
        }

        public IFileInfo GetDirectoryInfo(string path)
        {
            throw new NotImplementedException();
        }

        public IFileInfo GetFileInfo(string path)
        {
            throw new NotImplementedException();
        }

        public DateTime GetFileLastWriteTimeUtc(string path)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<DirectoryInfo> ListDirectories(string path)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IFileInfo> ListFiles(string path)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IFileInfo> ListFiles(string path, Matcher matcher)
        {
            throw new NotImplementedException();
        }

        public Stream OpenFile(string path)
        {
            throw new NotImplementedException();
        }

        public string ReadFile(string path)
        {
            throw new NotImplementedException();
        }

        public void StoreFile(string sourceFileName, string destinationPath)
        {
            throw new NotImplementedException();
        }

        DirectoryInfo IOrchardFileSystem.GetDirectoryInfo(string path)
        {
            throw new NotImplementedException();
        }

        DateTimeOffset IOrchardFileSystem.GetFileLastWriteTimeUtc(string path)
        {
            throw new NotImplementedException();
        }
    }
}
