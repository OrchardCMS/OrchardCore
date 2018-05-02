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
    /// This custom <see cref="IFileProvider"/> implementation provides the file contents of files
    /// whose path is under a Module Project 'wwwroot' folder, and while in a development environment.
    /// </summary>
    public class ModuleProjectStaticFileProvider : IFileProvider
    {
        private static Dictionary<string, string> _paths;
        private static object _synLock = new object();

        public ModuleProjectStaticFileProvider(IHostingEnvironment environment)
        {
            if (_paths != null)
            {
                return;
            }

            lock (_synLock)
            {
                if (_paths == null)
                {
                    //var assets = new List<Asset>();
                    var application = environment.GetApplication();

                    var paths = new Dictionary<string, string>();

                    foreach (var name in application.ModuleNames)
                    {
                        var module = environment.GetModule(name);

                        if (module.Assembly == null || Path.GetDirectoryName(module.Assembly.Location)
                            != Path.GetDirectoryName(application.Assembly.Location))
                        {
                            continue;
                        }

                        var contentRoot = Application.ModulesRoot + name + '/' + Module.ContentRoot;

                        var assets = module.Assets.Where(a => a.ModuleAssetPath
                            .StartsWith(contentRoot, StringComparison.Ordinal)).ToArray();

                        foreach (var asset in assets)
                        {
                            var requestPath = name + asset.ModuleAssetPath.Substring(contentRoot.Length - 1);
                            paths[requestPath] = asset.ProjectAssetPath;
                        }
                    }

                    _paths = paths;
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

            if (_paths.TryGetValue(path, out var projectAssetPath))
            {
                return new PhysicalFileInfo(new FileInfo(projectAssetPath));
            }

            return new NotFoundFileInfo(subpath);
        }

        public IChangeToken Watch(string filter)
        {
            return NullChangeToken.Singleton;
        }

        private string NormalizePath(string path)
        {
            return path.Replace('\\', '/').Trim('/').Replace("//", "/");
        }
    }
}
