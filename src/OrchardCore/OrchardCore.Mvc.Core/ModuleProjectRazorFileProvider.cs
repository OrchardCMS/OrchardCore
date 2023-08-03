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
        private static readonly object _synLock = new();

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
                            var index = asset.ModuleAssetPath.IndexOf(module.Root, StringComparison.Ordinal);

                            // Resolve the physical "{ModuleProjectDirectory}" from the project asset.
                            var filePath = asset.ModuleAssetPath[(index + module.Root.Length)..];
                            var root = asset.ProjectAssetPath[..^filePath.Length];

                            // Get the first module project asset which is under a "Pages" folder.
                            var page = assets.FirstOrDefault(a => a.ProjectAssetPath.Contains("/Pages/"));

                            // Check if the module project may have a razor page.
                            if (page != null && Directory.Exists(root))
                            {
                                // Razor pages are not watched in the same way as other razor views.
                                // We need a physical file provider on the "{ModuleProjectDirectory}".
                                _pageFileProviders.Add(new PhysicalFileProvider(root));
                            }

                            // Add the module project root.
                            roots[module.Name] = root;
                        }
                    }

                    _roots = roots;
                }
            }
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            // 'GetDirectoryContents()' is used to discover shapes templates and build fixed binding tables.
            // So the embedded file provider can always provide the structure under modules "Views" folders.

            // The razor view engine also uses 'GetDirectoryContents()' to find razor pages (not mvc views).
            // So here, we only need to serve the directory contents under modules projects "Pages" folders.

            // Note: This provider is not used in production where all modules precompiled views are used.
            // Note: See 'ApplicationRazorFileProvider' for the specific case of the application's module.

            if (subpath == null)
            {
                return NotFoundDirectoryContents.Singleton;
            }

            var folder = NormalizePath(subpath);

            // Under "Areas/{ModuleId}" or "Areas/{ModuleId}/**".
            if (folder.StartsWith(Application.ModulesRoot, StringComparison.Ordinal))
            {
                // Remove "Areas/" from the folder path.
                folder = folder[Application.ModulesRoot.Length..];
                var index = folder.IndexOf('/');

                // "{ModuleId}/**".
                if (index != -1)
                {
                    // Resolve the module id.
                    var module = folder[..index];

                    // Try to get the module project root.
                    if (_roots.TryGetValue(module, out var root) &&
                        // Check for a final or an intermadiate "Pages" segment.
                        (folder.EndsWith("/Pages", StringComparison.Ordinal) || folder.Contains("/Pages/")))
                    {
                        // Resolve the subpath relative to "{ModuleProjectDirectory}".
                        folder = String.Concat(root, folder.AsSpan(module.Length + 1));

                        if (Directory.Exists(folder))
                        {
                            // Serve the contents from the file system.
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

            // "Areas/**/*.*".
            if (path.StartsWith(Application.ModulesRoot, StringComparison.Ordinal))
            {
                // Skip the "Areas/" root folder.
                path = path[Application.ModulesRoot.Length..];
                var index = path.IndexOf('/');

                // "{ModuleId}/**/*.*".
                if (index != -1)
                {
                    // Resolve the module id.
                    var module = path[..index];

                    // Get the module root folder.
                    if (_roots.TryGetValue(module, out var root))
                    {
                        // Resolve "{ModuleProjectDirectory}**/*.*".
                        var filePath = String.Concat(root, path.AsSpan(module.Length + 1));

                        if (File.Exists(filePath))
                        {
                            //Serve the file from the physical file system.
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

            // "Areas/**/*.*".
            if (path.StartsWith(Application.ModulesRoot, StringComparison.Ordinal) && !path.Contains('*'))
            {
                // Skip the "Areas/" root folder.
                path = path[Application.ModulesRoot.Length..];
                var index = path.IndexOf('/');

                // "{ModuleId}/**/*.*".
                if (index != -1)
                {
                    // Resolve the module id.
                    var module = path[..index];

                    // Get the module root folder.
                    if (_roots.TryGetValue(module, out var root))
                    {
                        // Resolve "{ModuleProjectDirectory}**/*.*".
                        var filePath = String.Concat(root, path.AsSpan(module.Length + 1));

                        var directory = Path.GetDirectoryName(filePath);
                        var fileName = Path.GetFileNameWithoutExtension(filePath);

                        if (Directory.Exists(directory))
                        {
                            // Watch the project asset from the physical file system.
                            // Note: Here a wildcard is used on the file extension
                            // so that removing and re-adding a file is detected.
                            return new PollingWildCardChangeToken(directory, fileName + ".*");
                        }
                    }
                }
            }

            // The view engine uses a watch on "Pages/**/*.cshtml" but only for razor pages.
            // So here, we only use file providers for modules which have a "Pages" folder.
            else if (path.Equals("Pages/**/*.cshtml"))
            {
                var changeTokens = new List<IChangeToken>();

                // For each module which might have pages.
                foreach (var provider in _pageFileProviders)
                {
                    // Watch all razor files under its "Pages" folder.
                    var changeToken = provider.Watch("Pages/**/*.cshtml");

                    if (changeToken != null)
                    {
                        changeTokens.Add(changeToken);
                    }
                }

                if (changeTokens.Count == 1)
                {
                    return changeTokens.First();
                }

                if (changeTokens.Count > 0)
                {
                    // Use a composite of all provider tokens.
                    return new CompositeChangeToken(changeTokens);
                }
            }

            return NullChangeToken.Singleton;
        }

        private static string NormalizePath(string path) => path.Replace('\\', '/').Trim('/');
    }
}
