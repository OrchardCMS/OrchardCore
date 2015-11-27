using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Orchard.FileSystem;
using Microsoft.AspNet.Http;

namespace Orchard.Tests.Stubs
{
    public class StubWebSiteFolder : IClientFolder
    {
        public IEnumerable<string> ListDirectories(string path)
        {
            if (!Directory.Exists(path))
                return Enumerable.Empty<string>();

            return Directory.GetDirectories(path);
        }

        public IEnumerable<string> ListFiles(string path, bool recursive)
        {
            if (!Directory.Exists(path))
                return Enumerable.Empty<string>();

            return Directory.GetFiles(path);
        }

        public bool FileExists(string virtualPath)
        {
            throw new NotImplementedException();
        }

        public string ReadFile(string path)
        {
            return ReadFile(path, false);
        }

        public string ReadFile(string path, bool actualContent)
        {
            if (!File.Exists(path))
                return null;

            return File.ReadAllText(path);
        }

        public void CopyFileTo(string virtualPath, Stream destination)
        {
            throw new NotImplementedException();
        }

        public void CopyFileTo(string virtualPath, Stream destination, bool actualContent)
        {
            throw new NotImplementedException();
        }
    }
}