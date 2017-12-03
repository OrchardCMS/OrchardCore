using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Primitives;

namespace OrchardCore.Modules
{
    /// <summary>
    /// This custom <see cref="IFileProvider"/> implementation provides the file contents
    /// of Module Project Content files while in a development environment.
    /// </summary>
    public class ModuleProjectContentFileProvider : IFileProvider
    {
        private static Dictionary<string, string> _paths;
        private static object _synLock = new object();

        private string _contentPath;

        public ModuleProjectContentFileProvider(IHostingEnvironment environment, string contentPath)
        {
            _contentPath = NormalizePath(contentPath) + "/";

            if (_paths != null)
            {
                return;
            }

            lock (_synLock)
            {
                if (_paths == null)
                {
                    var paths = new List<KeyValuePair<string, string>>();
                    var mainAssembly = environment.LoadApplicationAssembly();

                    foreach (var moduleId in environment.GetModuleNames())
                    {
                        var assembly = environment.LoadModuleAssembly(moduleId);

                        if (assembly == null || Path.GetDirectoryName(assembly.Location)
                            != Path.GetDirectoryName(mainAssembly.Location))
                        {
                            continue;
                        }

                        paths.AddRange(environment.GetModuleAssetsMap(moduleId).Where(x => x.Key
                            .StartsWith("Modules/" + moduleId + "/Content/", StringComparison.Ordinal)));
                    }

                    _paths = new Dictionary<string, string>(paths.ToDictionary(x => x.Key, x => x.Value));
                }
            }
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            return NotFoundDirectoryContents.Singleton;
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            if (subpath == null)
            {
                return new NotFoundFileInfo(subpath);
            }

            var path = _contentPath + NormalizePath(subpath);

            if (_paths.ContainsKey(path))
            {
                return new PhysicalFileInfo(new FileInfo(_paths[path]));
            }

            return new NotFoundFileInfo(subpath);
        }

        public IChangeToken Watch(string filter)
        {
            return NullChangeToken.Singleton;
        }

        private string NormalizePath(string path)
        {
            return path.Replace('\\', '/').Trim('/');
        }
    }
}
