using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Primitives;
using OrchardCore.Modules;

namespace OrchardCore.DisplayManagement.Liquid
{
    /// <summary>
    /// This custom <see cref="IFileProvider"/> implementation provides the file contents
    /// of Module Project Liquid files while in a development environment.
    /// </summary>
    public class ModuleProjectLiquidFileProvider : IFileProvider
    {
        private static Dictionary<string, string> _paths;
        private static object _synLock = new object();

        public ModuleProjectLiquidFileProvider(IHostingEnvironment environment)
        {
            if (_paths != null)
            {
                return;
            }

            lock (_synLock)
            {
                if (_paths == null)
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
                                .EndsWith(".liquid", StringComparison.Ordinal)));
                        }

                        _paths = new Dictionary<string, string>(paths.ToDictionary(x => x.Key, x => x.Value));
                    }
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

            var path = NormalizePath(subpath);

            if (_paths.ContainsKey(path))
            {
                return new PhysicalFileInfo(new FileInfo(_paths[path]));
            }

            return new NotFoundFileInfo(subpath);
        }

        public IChangeToken Watch(string filter)
        {
            if (filter == null)
            {
                return NullChangeToken.Singleton;
            }

            var path = NormalizePath(filter);

            if (_paths.ContainsKey(path))
            {
                return new PollingFileChangeToken(new FileInfo(_paths[path]));
            }

            return NullChangeToken.Singleton;
        }

        private string NormalizePath(string path)
        {
            return path.Replace('\\', '/').Trim('/');
        }
    }
}
