using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private static readonly object _synLock = new();

        public ModuleProjectLiquidFileProvider(IApplicationContext applicationContext)
        {
            if (_paths != null)
            {
                return;
            }

            lock (_synLock)
            {
                if (_paths == null)
                {
                    var assets = new List<Asset>();
                    var application = applicationContext.Application;

                    foreach (var module in application.Modules)
                    {
                        // If the module and the application assemblies are not at the same location,
                        // this means that the module is referenced as a package, not as a project in dev.
                        if (module.Assembly == null || Path.GetDirectoryName(module.Assembly.Location)
                            != Path.GetDirectoryName(application.Assembly.Location))
                        {
                            continue;
                        }

                        // Get module assets which are liquid template files.
                        assets.AddRange(module.Assets.Where(a => a.ModuleAssetPath
                            .EndsWith(".liquid", StringComparison.Ordinal)));
                    }

                    // Init the mapping between module and project asset paths.
                    _paths = assets.ToDictionary(a => a.ModuleAssetPath, a => a.ProjectAssetPath);
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

            // Map the module asset path to the project asset path.
            if (_paths.TryGetValue(path, out var projectAssetPath))
            {
                // Serve the project asset from the physical file system.
                return new PhysicalFileInfo(new FileInfo(projectAssetPath));
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

            // Map the module asset path to the project asset path.
            if (_paths.TryGetValue(path, out var projectAssetPath))
            {
                // Watch the project asset from the physical file system.
                return new PollingFileChangeToken(new FileInfo(projectAssetPath));
            }

            return NullChangeToken.Singleton;
        }

        private static string NormalizePath(string path) => path.Replace('\\', '/').Trim('/');
    }
}
