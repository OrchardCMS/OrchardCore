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
        private const string RootFolder = "Modules";
        private const string MappingFileName = "ModuleAssetPaths.map";
        private const string ModulesFileName = "ModuleAssembliesNames.map";

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

                var paths = new List<string>();

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

                var modules = new List<string>();
                fileInfo = fileProvider.GetFileInfo(ModulesFileName);

                if (fileInfo?.Exists ?? false)
                {
                    using (var reader = fileInfo.CreateReadStream())
                    {
                        using (var sr = new StreamReader(reader))
                        {
                            string line;
                            while ((line = sr.ReadLine()) != null)
                            {
                                modules.Add(line);
                            }
                        }
                    }
                }

                _paths = paths;
                _modules = modules;
            }
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            if (subpath == null)
            {
                return null;
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
                //var moduleId = subPath.Substring(0, subPath.IndexOf("/"));
                //var fileProvider = new EmbeddedFileProvider(Assembly.Load(moduleId));
                //var paths = fileProvider.GetFileInfo(MappingFileName);
            }

            foreach (var path in _paths.Where(x => x.StartsWith(subPath)).ToList())
            {
                var trailingPath = path.Replace(subPath + '/', "");

                if (trailingPath.IndexOf('/') == -1)
                {
                    entries.Add(GetFileInfo(path));
                }
                else
                {
                    folders.Add(trailingPath.Substring(0, trailingPath.IndexOf('/')));
                }
            }

            entries.AddRange(folders.Distinct().Select(x => new EmbeddedDirectoryInfo(x)));

            return new EmbeddedDirectoryContents(entries);
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            if (subpath == null)
            {
                return null;
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

            return null;
        }

        public IChangeToken Watch(string filter)
        {
            return null;
        }
    }
}
