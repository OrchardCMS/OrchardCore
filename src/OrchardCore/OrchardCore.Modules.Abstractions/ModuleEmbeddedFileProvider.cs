using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Internal;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Primitives;
using OrchardCore.Modules.FileProviders;

namespace OrchardCore.Modules
{
    /// <summary>
    /// This custom <see cref="IFileProvider"/> implementation provides the file contents
    /// of embedded files in Module assemblies.
    /// </summary>
    public class ModuleEmbeddedFileProvider : IFileProvider
    {
        private static IFileProvider _rootFileProvider;
        private static object _synLock = new object();

        private readonly IHostingEnvironment _environment;

        public ModuleEmbeddedFileProvider(IHostingEnvironment hostingEnvironment)
        {
            _environment = hostingEnvironment;

            if (_rootFileProvider != null)
            {
                return;
            }

            lock (_synLock)
            {
                if (_rootFileProvider == null)
                {
                    _rootFileProvider = new PhysicalFileProvider(_environment.ContentRootPath);
                }
            }
        }

        private Application Application => _environment.GetApplication();

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            if (subpath == null)
            {
                return NotFoundDirectoryContents.Singleton;
            }

            var folder = NormalizePath(subpath);

            var entries = new List<IFileInfo>();

            if (folder == "")
            {
                entries.Add(new EmbeddedDirectoryInfo(Application.ModulesPath));
            }
            else if (folder == Application.ModulesPath)
            {
                entries.AddRange(Application.ModuleNames
                    .Select(n => new EmbeddedDirectoryInfo(n)));
            }
            else if (folder == Application.ModulePath)
            {
                return new PhysicalDirectoryContents(Application.Path);
            }
            else if (folder.StartsWith(Application.ModuleRoot, StringComparison.Ordinal))
            {
                var tokenizer = new StringTokenizer(folder, new char[] { '/' });
                if (tokenizer.Any(s => s == "Pages" || s == "Views"))
                {
                    var folderSubPath = folder.Substring(Application.ModuleRoot.Length);
                    return new PhysicalDirectoryContents(Application.Root + folderSubPath);
                }
            }
            else if (folder.StartsWith(Application.ModulesRoot, StringComparison.Ordinal))
            {
                var path = folder.Substring(Application.ModulesRoot.Length);
                var index = path.IndexOf('/');
                var name = index == -1 ? path : path.Substring(0, index);
                var assetPaths = _environment.GetModule(name).AssetPaths;
                var folders = new HashSet<string>(StringComparer.Ordinal);
                var folderSlash = folder + '/';

                foreach (var assetPath in assetPaths.Where(a => a.StartsWith(folderSlash, StringComparison.Ordinal)))
                {
                    var folderPath = assetPath.Substring(folderSlash.Length);
                    var pathIndex = folderPath.IndexOf('/');
                    var isFilePath = pathIndex == -1;

                    if (isFilePath)
                    {
                        entries.Add(GetFileInfo(assetPath));
                    }
                    else
                    {
                        folders.Add(folderPath.Substring(0, pathIndex));
                    }
                }

                entries.AddRange(folders.Select(f => new EmbeddedDirectoryInfo(f)));
            }

            return new EmbeddedDirectoryContents(entries);
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            if (subpath == null)
            {
                return new NotFoundFileInfo(subpath);
            }

            var path = NormalizePath(subpath);

            if (path.StartsWith(Application.ModuleRoot, StringComparison.Ordinal))
            {
                var fileSubPath = path.Substring(Application.ModuleRoot.Length);
                return new PhysicalFileInfo(new FileInfo(Application.Root + fileSubPath));
            }
            else if (path.StartsWith(Application.ModulesRoot, StringComparison.Ordinal))
            {
                path = path.Substring(Application.ModulesRoot.Length);
                var index = path.IndexOf('/');

                if (index != -1)
                {
                    var module = path.Substring(0, index);
                    var fileSubPath = path.Substring(index + 1);
                    return _environment.GetModule(module).GetFileInfo(fileSubPath);
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

            if (path.StartsWith(Application.ModuleRoot, StringComparison.Ordinal))
            {
                var fileSubPath = path.Substring(Application.ModuleRoot.Length);
                return new PollingFileChangeToken(new FileInfo(Application.Root + fileSubPath));
            }

            if (path.Equals("**/*.cshtml"))
            {
                return _rootFileProvider.Watch("Pages/**/*.cshtml");
            }

            return NullChangeToken.Singleton;
        }

        private string NormalizePath(string path)
        {
            return path.Replace('\\', '/').Trim('/').Replace("//", "/");
        }
    }
}