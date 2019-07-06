using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Primitives;

namespace OrchardCore.Modules
{
    /// <summary>
    /// This custom <see cref="IFileProvider"/> implementation provides the file contents of files
    /// whose path is under a Module Project 'wwwroot' folder, and while in a development environment.
    /// </summary>
    public class ModuleProjectStaticFileProvider : IModuleStaticFileProvider
    {
        private static Dictionary<string, string> _roots;
        private static readonly object _synLock = new object();

        public ModuleProjectStaticFileProvider(IApplicationContext applicationContext)
        {
            if (_roots != null)
            {
                return;
            }

            lock (_synLock)
            {
                if (_roots == null)
                {
                    var application = applicationContext.Application;

                    var roots = new Dictionary<string, string>();

                    // Resolve all module projects "wwwroot".
                    foreach (var module in application.Modules)
                    {
                        // If the module and the application assemblies are not at the same location,
                        // this means that the module is referenced as a package, not as a project in dev.
                        if (module.Assembly == null || Path.GetDirectoryName(module.Assembly.Location)
                            != Path.GetDirectoryName(application.Assembly.Location))
                        {
                            continue;
                        }

                        // Get the 1st module asset under "Areas/{ModuleId}/wwwroot/".
                        var asset = module.Assets.FirstOrDefault(a => a.ModuleAssetPath
                            .StartsWith(module.Root + Module.WebRoot, StringComparison.Ordinal));

                        if (asset != null)
                        {
                            // Resolve "{ModuleProjectDirectory}wwwroot/" from the project asset.
                            var index = asset.ProjectAssetPath.IndexOf('/' + Module.WebRoot);

                            // Add the module project "wwwroot" folder.
                            roots[module.Name] = asset.ProjectAssetPath.Substring(0, index + Module.WebRoot.Length + 1);
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

            // "{ModuleId}/**/*.*".
            if (index != -1)
            {
                // Resolve the module id.
                var module = path.Substring(0, index);

                // Get the module project "wwwroot" folder.
                if (_roots.TryGetValue(module, out var root))
                {
                    // Resolve "{ModuleProjectDirectory}wwwroot/**/*.*"
                    var filePath = root + path.Substring(module.Length + 1);

                    if (File.Exists(filePath))
                    {
                        //Serve the file from the physical file system.
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