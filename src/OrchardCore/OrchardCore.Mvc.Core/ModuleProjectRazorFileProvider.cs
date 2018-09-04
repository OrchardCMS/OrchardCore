using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
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
        private static Dictionary<string, IFileProvider> _rootFileProviders;
        private static Dictionary<string, string> _roots;
        private static object _synLock = new object();

        public ModuleProjectRazorFileProvider(IHostingEnvironment environment)
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

                    _rootFileProviders = new Dictionary<string, IFileProvider>();
                    var roots = new Dictionary<string, string>();

                    foreach (var name in application.ModuleNames)
                    {
                        var module = environment.GetModule(name);

                        if (module.Assembly == null || Path.GetDirectoryName(module.Assembly.Location)
                            != Path.GetDirectoryName(application.Assembly.Location))
                        {
                            continue;
                        }

                        var assets = module.Assets.Where(a => a.ModuleAssetPath
                            .EndsWith(".cshtml", StringComparison.Ordinal));

                        if (assets.Any())
                        {
                            var asset = assets.First();
                            var index = asset.ModuleAssetPath.IndexOf(module.Root);

                            var filePath = asset.ModuleAssetPath.Substring(index + module.Root.Length);
                            var root = asset.ProjectAssetPath.Substring(0, asset.ProjectAssetPath.Length - filePath.Length);

                            var page = assets.FirstOrDefault(a => a.ProjectAssetPath.Contains("/Pages/"));

                            if (page != null)
                            {
                                _rootFileProviders[name] = (new PhysicalFileProvider(root));
                            }

                            roots[name] = root;
                        }
                    }

                    _roots = roots;
                }
            }
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            if (subpath == null)
            {
                return NotFoundDirectoryContents.Singleton;
            }

            var folder = NormalizePath(subpath);
            var index = folder.IndexOf(Application.ModulesRoot);

            if (index != -1)
            {
                folder = folder.Substring(Application.ModulesRoot.Length);
                index = folder.IndexOf('/');

                if (index != -1)
                {
                    var module = folder.Substring(0, index);

                    if (_roots.TryGetValue(module, out var root) &&
                        (folder.Contains("/Pages/") || folder.EndsWith("/Pages")))
                    {
                        folder = root + folder.Substring(module.Length + 1);

                        if (Directory.Exists(folder))
                        {
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

                foreach (var provider in _rootFileProviders.Values)
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
