using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Internal;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Primitives;
using OrchardCore.Modules;

namespace OrchardCore.Mvc
{
    /// <summary>
    /// This custom <see cref="IFileProvider"/> implementation provides the file contents
    /// of Module Project Razor files while in a development environment.
    /// </summary>
    public class ModuleProjectRazorFileProvider : IFileProvider
    {
        private static IList<IFileProvider> _pageFileProviders;
        private static Dictionary<string, string> _roots;
        private static object _synLock = new object();

        public ModuleProjectRazorFileProvider(IApplicationContext applicationContext)
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

                    _pageFileProviders = new List<IFileProvider>();
                    var roots = new Dictionary<string, string>();

                    // Resolve all module projects roots.
                    foreach (var module in application.Modules)
                    {
                        // If the module and the application assemblies are not at the same location,
                        // this means that the module is referenced as a package, not as a project in dev.
                        if (module.Assembly == null || Path.GetDirectoryName(module.Assembly.Location)
                            != Path.GetDirectoryName(application.Assembly.Location))
                        {
                            continue;
                        }

                        // Get module assets which are razor files.
                        var assets = module.Assets.Where(a => a.ModuleAssetPath
                            .EndsWith(".cshtml", StringComparison.Ordinal));

                        if (assets.Any())
                        {
                            var asset = assets.First();
                            var index = asset.ModuleAssetPath.IndexOf(module.Root);

                            // Resolve the physical "{ModuleProjectDirectory}" from the project asset.
                            var filePath = asset.ModuleAssetPath.Substring(index + module.Root.Length);
                            var root = asset.ProjectAssetPath.Substring(0, asset.ProjectAssetPath.Length - filePath.Length);

                            // Get the first module project asset which is under a "Pages" folder.
                            var page = assets.FirstOrDefault(a => a.ProjectAssetPath.Contains("/Pages/"));

                            // Check if the module project may have a razor page.
                            if (page != null)
                            {
                                // Razor pages are not watched in the same way as other razor views.
                                // We need a physical file provider on the "{ModuleProjectDirectory}".
                                _pageFileProviders.Add(new PhysicalFileProvider(root));
                            }

                            roots[module.Name] = root;
                        }
                    }

                    _roots = roots;
                }
            }
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            // The razor view engine uses 'GetDirectoryContents()' only for razor pages.
            // So here, we only serve contents under the module projects "Pages" folders.

            if (subpath == null)
            {
                return NotFoundDirectoryContents.Singleton;
            }

            var folder = NormalizePath(subpath);

            // Under ".Modules/{ModuleId}" or ".Modules/{ModuleId}/**".
            if (folder.StartsWith(Application.ModulesRoot, StringComparison.Ordinal))
            {
                // Remove ".Modules/" from the folder path.
                folder = folder.Substring(Application.ModulesRoot.Length);
                var index = folder.IndexOf('/');

                if (index != -1)
                {
                    // Resolve the module id.
                    var module = folder.Substring(0, index);

                    // Get the module project root and check if under a "Pages" folder.
                    if (_roots.TryGetValue(module, out var root) &&
                        (folder.Contains("/Pages/") || folder.EndsWith("/Pages")))
                    {
                        // Resolve the page folder: "{ModuleProjectDirectory}/Pages".
                        // Or any other subfolders: "{ModuleProjectDirectory}/Pages/**".
                        folder = root + folder.Substring(module.Length + 1);

                        if (Directory.Exists(folder))
                        {
                            //Serve the contents from the file system.
                            return new PhysicalDirectoryContents(folder);
                        }
                    }
                }
            }

            return NotFoundDirectoryContents.Singleton;
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            if (subpath == null)
            {
                return new NotFoundFileInfo(subpath);
            }

            var path = NormalizePath(subpath);
            var index = path.IndexOf(Application.ModulesRoot);

            if (index != -1)
            {
                path = path.Substring(Application.ModulesRoot.Length);
                index = path.IndexOf('/');

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
            var index = path.IndexOf(Application.ModulesRoot);

            if (index != -1)
            {
                path = path.Substring(Application.ModulesRoot.Length);
                index = path.IndexOf('/');

                if (index != -1)
                {
                    var module = path.Substring(0, index);

                    if (_roots.TryGetValue(module, out var root))
                    {
                        var filePath = root + path.Substring(module.Length + 1);

                        var directory = Path.GetDirectoryName(filePath);
                        var fileName = Path.GetFileNameWithoutExtension(filePath);

                        if (Directory.Exists(directory))
                        {
                            return new PollingWildCardChangeToken(directory, fileName + ".*");
                        }
                    }
                }
            }

            if (path.Equals("**/*.cshtml"))
            {
                var changeTokens = new List<IChangeToken>();

                foreach (var provider in _pageFileProviders)
                {
                    var changeToken = provider.Watch("Pages/**/*.cshtml");

                    if (changeToken != null)
                    {
                        changeTokens.Add(changeToken);
                    }
                }

                if (changeTokens.Count > 0)
                {
                    return new CompositeChangeToken(changeTokens);
                }
            }

            return NullChangeToken.Singleton;
        }

        private string NormalizePath(string path)
        {
            return path.Replace('\\', '/').Trim('/');
        }
    }
}
