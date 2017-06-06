using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Primitives;

namespace Orchard.Mvc
{
    /// <summary>
    /// This custom <see cref="IFileProvider"/> implementation provides the file contents
    /// of Module Project Razor files while in a development environment.
    /// </summary>
    public class ModuleProjectRazorFileProvider : IFileProvider
    {
        private static ConcurrentDictionary<string, string> _paths;
        private static object _synLock = new object();

        private IHostingEnvironment _hostingEnvironment;

        public ModuleProjectRazorFileProvider(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;

            if (_paths == null)
            {
                lock (_synLock)
                {
                    if (_paths == null)
                    {
                        var path = Path.Combine(hostingEnvironment.ContentRootPath, "obj",
                            hostingEnvironment.ApplicationName + ".ModuleProjectRazorFilesMapping.txt");

                        if (File.Exists(path))
                        {
                            var paths = File.ReadAllLines(path)
                                .Select(x => x.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
                                .Where(x => x.Count() == 2).ToDictionary(x => x[1].Replace('\\', '/'), x => x[0]);

                            _paths = new ConcurrentDictionary<string, string>(paths);
                        }
                        else
                        {
                            _paths = new ConcurrentDictionary<string, string>();
                        }
                    }
                }
            }
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            return null;
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            if (_paths.TryGetValue(subpath, out var path))
            {
                return new PhysicalFileInfo(new FileInfo(path));
            }
            
            return null;
        }

        public IChangeToken Watch(string filter)
        {
            if (_paths.TryGetValue(filter, out var path))
            {
                return new PollingFileChangeToken(new FileInfo(path));
            }

            return null;
        }
    }
}
