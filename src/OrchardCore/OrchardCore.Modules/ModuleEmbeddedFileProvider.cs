using System.Collections.Generic;
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
        private const string RootFolder = "Modules";
        private const string ModuleAssetsMap = "module.assets.map";
        private const string ModulesNamesMap = "module.names.map";

        private static List<string> _paths;
        private static List<string> _modules;
        private static object _synLock = new object();

        private IHostingEnvironment _hostingEnvironment;
        private string _subPath;

        public ModuleEmbeddedFileProvider(IHostingEnvironment hostingEnvironment, string subPath = null)
        {
            _hostingEnvironment = hostingEnvironment;
            _subPath = subPath?.Replace('\\', '/').Trim('/') ?? "";

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

                var fileProvider = new EmbeddedFileProvider(Assembly.Load(new AssemblyName(_hostingEnvironment.ApplicationName)));

                var fileInfo = fileProvider.GetFileInfo(ModulesNamesMap);
                _modules = fileInfo.ReadAllLines().ToList();

                var paths = new List<string>();

                foreach (var module in _modules)
                {
                    fileProvider = new EmbeddedFileProvider(Assembly.Load(module));
                    fileInfo = fileProvider.GetFileInfo(ModuleAssetsMap);

                    var assetPaths = fileInfo.ReadAllLines().Select(x => x.Replace('\\', '/')).ToList();
                    assetPaths.RemoveAt(0);

                    paths.AddRange(assetPaths);
                }

                _paths = paths.Select(x => x.Replace('\\', '/')).ToList();
            }
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            if (subpath == null)
            {
                return NotFoundDirectoryContents.Singleton;
            }

            var subPath = subpath.Replace('\\', '/').Trim('/');

            if (_subPath.Length > 0)
            {
                subPath = _subPath + '/' + subPath;
            }

            var entries = new List<IFileInfo>();
            var folders = new List<string>();

            if (subPath == "")
            {
                entries.Add(new EmbeddedDirectoryInfo(RootFolder));
                return new EmbeddedDirectoryContents(entries);
            }

            if (subPath == RootFolder)
            {
                entries.AddRange(_modules.Select(x => new EmbeddedDirectoryInfo(x)));
                return new EmbeddedDirectoryContents(entries);
            }

            if (subPath.StartsWith(RootFolder))
            {
                subPath = subPath.Substring(RootFolder.Length + 1);

                var index = subPath.IndexOf("/");
                var moduleId = index == -1 ? subPath : subPath.Substring(0, subPath.IndexOf("/"));

                var fileProvider = new EmbeddedFileProvider(Assembly.Load(moduleId));
                var fileInfo = fileProvider.GetFileInfo(ModuleAssetsMap);

                var paths = fileInfo.ReadAllLines().Select(x => x.Replace('\\', '/')).ToList();

                foreach (var path in paths.Where(x => x.StartsWith(RootFolder + '/' + subPath)))
                {
                    var trailingPath = path.Replace(RootFolder + '/' + subPath + '/', "");

                    if (trailingPath.IndexOf('/') == -1)
                    {
                        entries.Add(GetFileInfo(path));
                    }
                    else
                    {
                        folders.Add(trailingPath.Substring(0, trailingPath.IndexOf('/')));
                    }
                }
            }

            entries.AddRange(folders.Distinct().Select(x => new EmbeddedDirectoryInfo(x)));

            return new EmbeddedDirectoryContents(entries);
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            if (subpath == null)
            {
                return new NotFoundFileInfo(subpath);
            }

            var subPath = subpath.Replace('\\', '/').Trim('/');

            if (_subPath.Length > 0)
            {
                subPath = _subPath + '/' + subPath;
            }

            if (_paths.Contains(subPath))
            {
                subPath = subPath.Substring(RootFolder.Length + 1);
                var moduleId = subPath.Substring(0, subPath.IndexOf("/"));
                var fileProvider = new EmbeddedFileProvider(Assembly.Load(moduleId));
                return fileProvider.GetFileInfo(subPath.Substring(moduleId.Length + 1));
            }

            return new NotFoundFileInfo(subpath);
        }

        public IChangeToken Watch(string filter)
        {
            return NullChangeToken.Singleton;
        }
    }
}
