using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Microsoft.AspNet.Hosting;
using Orchard.FileSystem.VirtualPath;

namespace Orchard.FileSystem.WebSite
{
    /// <summary>
    /// TODO: Take this out and move it to Orchard.FileSystem.WebHosting
    /// </summary>
    public class WebSiteFolder : IClientFolder
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IVirtualPathProvider _virtualPathProvider;

        public WebSiteFolder(IHostingEnvironment hostingEnvironment,
            IVirtualPathProvider virtualPathProvider)
        {
            _hostingEnvironment = hostingEnvironment;
            _virtualPathProvider = virtualPathProvider;
        }

        public IEnumerable<string> ListDirectories(string virtualPath)
        {
            if (!_virtualPathProvider.DirectoryExists(virtualPath))
            {
                return Enumerable.Empty<string>();
            }

            return _virtualPathProvider.ListDirectories(virtualPath);
        }

        private IEnumerable<string> ListFiles(IEnumerable<string> directories)
        {
            return directories.SelectMany(d => ListFiles(d, true));
        }

        public IEnumerable<string> ListFiles(string virtualPath, bool recursive)
        {
            if (!recursive)
            {
                return _virtualPathProvider.ListFiles(virtualPath);
            }
            return _virtualPathProvider.ListFiles(virtualPath).Concat(ListFiles(ListDirectories(virtualPath)));
        }

        public bool FileExists(string virtualPath)
        {
            return _virtualPathProvider.FileExists(virtualPath);
        }

        public string ReadFile(string virtualPath)
        {
            return ReadFile(virtualPath, false);
        }

        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public string ReadFile(string virtualPath, bool actualContent)
        {
            if (!_virtualPathProvider.FileExists(virtualPath))
            {
                return null;
            }

            if (actualContent)
            {
                var physicalPath = _virtualPathProvider.MapPath(virtualPath);
                return File.ReadAllText(physicalPath);
            }
            else
            {
                return _virtualPathProvider.ReadFile(Normalize(virtualPath));
            }
        }

        public void CopyFileTo(string virtualPath, Stream destination)
        {
            CopyFileTo(virtualPath, destination, false/*actualContent*/);
        }

        public void CopyFileTo(string virtualPath, Stream destination, bool actualContent)
        {
            if (actualContent)
            {
                // This is an unfortunate side-effect of the dynamic compilation work.
                // Orchard has a custom virtual path provider which adds "<@Assembly xxx@>"
                // directives to WebForm view files. There are cases when this side effect
                // is not expected by the consumer of the WebSiteFolder API.
                // The workaround here is to go directly to the file system.
                var physicalPath = _virtualPathProvider.MapPath(virtualPath);
                using (var stream = File.Open(physicalPath, FileMode.Open, FileAccess.Read))
                {
                    stream.CopyTo(destination);
                }
            }
            else
            {
                using (var stream = _virtualPathProvider.OpenFile(Normalize(virtualPath)))
                {
                    stream.CopyTo(destination);
                }
            }
        }

        string Normalize(string virtualPath)
        {
            return virtualPath.Replace(_hostingEnvironment.WebRootPath, "~").Replace('\\', '/');
        }
    }
}