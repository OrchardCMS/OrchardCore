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
        private static Dictionary<string, string> _roots;
        private static object _synLock = new object();

        public ModuleProjectStaticFileProvider(IHostingEnvironment environment)
        {
            if (_roots != null)
            {
                return;
            }

            lock (_synLock)
            {
                if (_roots == null)
                {
                    var application = environment.GetApplication();

                    var roots = new Dictionary<string, string>();

                    foreach (var name in application.ModuleNames)
                    {
                        var module = environment.GetModule(name);

                        if (module.Assembly == null || Path.GetDirectoryName(module.Assembly.Location)
                            != Path.GetDirectoryName(application.Assembly.Location))
                        {
                            continue;
                        }

                        var asset = module.Assets.FirstOrDefault(a => a.ModuleAssetPath
                            .StartsWith(module.Root + Module.WebRoot, StringComparison.Ordinal));

                        if (asset != null)
                        {
                            var index = asset.ProjectAssetPath.IndexOf('/' + Module.WebRoot);
                            roots[name] = asset.ProjectAssetPath.Substring(0, index + Module.WebRoot.Length + 1);
                        }
                    }

                    _roots = roots;
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
            var index = path.IndexOf('/');

            if (index != -1)
            {
                var module = path.Substring(0, index);

                if (_roots.TryGetValue(module, out var root))
                {
                    var filePath = root + path.Substring(module.Length + 1);

                    if (File.Exists(filePath))
                    {
                        return new PhysicalFileInfo(new FileInfo(filePath));
                    }
                }
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