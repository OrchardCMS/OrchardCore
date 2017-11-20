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
            _contentPath = '/' + contentPath.Replace('\\', '/').Trim('/');

            if (_paths != null)
            {
                return;
            }

            lock (_synLock)
            {
                if (_paths == null)
                {
                    var paths = new List<string>();
                    var mainAssembly = environment.LoadApplicationAssembly();

                    foreach (var moduleId in environment.GetModuleNames())
                    {
                        var assembly = environment.LoadModuleAssembly(moduleId);

                        if (assembly == null || Path.GetDirectoryName(assembly.Location)
                            != Path.GetDirectoryName(mainAssembly.Location))
                        {
                            continue;
                        }

                        var assetPaths = environment.GetModuleAssets(moduleId);
                        var projectFolder = assetPaths.FirstOrDefault();

                        if (Directory.Exists(projectFolder))
                        {
                            assetPaths = assetPaths.Skip(1).Where(x => x.StartsWith(
                                "Modules/" + moduleId + "/Content/")).ToList();

                            paths.AddRange(assetPaths.Select(x => projectFolder + "/"
                                + x.Substring(("Modules/" + moduleId).Length) + "|/" + x));
                        }
                    }

                    _paths = new Dictionary<string, string>(paths
                        .Select(x => x.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
                        .Where(x => x.Length == 2).ToDictionary(x => x[1], x => x[0]));
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

            var path = _contentPath + subpath;

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
    }
}
