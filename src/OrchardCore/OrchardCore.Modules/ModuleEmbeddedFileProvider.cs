using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using OrchardCore.Modules.FileProviders;

namespace OrchardCore.Modules
{
    /// <summary>
    /// This custom <see cref="IFileProvider"/> implementation provides the file contents
    /// of embedded files in Module assemblies.
    /// </summary>
    public class ModuleEmbeddedFileProvider : IFileProvider
    {
        private const string MappingFileName = "ModuleAssetPaths.map";

        private static List<string> _paths;
        private static List<string> _folders;
        private static object _synLock = new object();

        private IHostingEnvironment _hostingEnvironment;

        public ModuleEmbeddedFileProvider(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;

            if (_paths != null)
            {
                return;
            }

            lock (_synLock)
            {
                if (_paths != null)
                {
                    return;
                }

                var paths = new List<string>();
                _folders = new List<string>();

                var fileProvider = new EmbeddedFileProvider(Assembly.Load(new AssemblyName(_hostingEnvironment.ApplicationName)));
                var fileInfo = fileProvider.GetFileInfo(MappingFileName);

                if (fileInfo?.Exists ?? false)
                {
                    using (var reader = fileInfo.CreateReadStream())
                    {
                        using (var sr = new StreamReader(reader))
                        {
                            string line;
                            while ((line = sr.ReadLine()) != null)
                            {
                                paths.Add(line.Replace('\\', '/'));
                            }
                        }
                    }
                }

                _paths = paths;
            }
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            if (subpath == null)
            {
                return null;
            }

            var path = subpath.Replace('\\', '/').Trim('/');

            var entries = new List<IFileInfo>();
            var folders = new List<string>();

            if (path == "")
            {
                entries.Add(new EmbeddedDirectoryInfo("Modules"));
                return new EnumerableDirectoryContents(entries);
            }

            foreach (var test in _paths.Where(x => x.StartsWith(path)).ToList())
            {
                var test1 = test.Replace(path + '/', "");

                if (test1.IndexOf('/') == -1)
                {
                    entries.Add(GetFileInfo(test));
                }
                else
                {
                    folders.Add(test1.Substring(0, test1.IndexOf('/')));
                }
            }

            var toto = folders.Distinct();

            foreach (var folder in toto)
            {
                entries.Add(new EmbeddedDirectoryInfo(folder));
            }

            return new EnumerableDirectoryContents(entries);
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            if (subpath == null)
            {
                return null;
            }

            var path = subpath.Replace('\\', '/').TrimStart('/');

            if (_paths.Contains(path))
            {
                path = path.Replace("Modules/", "");
                var moduleId = path.Substring(0, path.IndexOf("/"));

                var fileProvider = new EmbeddedFileProvider(Assembly.Load(moduleId));
                var toto = path.Replace(moduleId, "").TrimStart('/');
                var test = fileProvider.GetFileInfo(path.Replace(moduleId, "").TrimStart('/'));
                return test;
            }

            return null;
        }

        public IChangeToken Watch(string filter)
        {
            return null;
        }
    }
}
