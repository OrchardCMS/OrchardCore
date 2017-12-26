using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
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
        private IHostingEnvironment _environment;
        private string _contentRoot;

        public ModuleEmbeddedFileProvider(IHostingEnvironment hostingEnvironment, string contentPath = null)
        {
            _environment = hostingEnvironment;
            _contentRoot = contentPath != null ? NormalizePath(contentPath) + '/' : "";
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            if (subpath == null)
            {
                return NotFoundDirectoryContents.Singleton;
            }

            var folder = _contentRoot + NormalizePath(subpath);

            var entries = new List<IFileInfo>();

            if (folder == "")
            {
                entries.Add(new EmbeddedDirectoryInfo(Application.ModulesPath));
            }
            else if (folder == Application.ModulesPath)
            {
                entries.AddRange(_environment.GetApplication().ModuleNames
                    .Select(n => new EmbeddedDirectoryInfo(n)));
            }
            else if (folder.StartsWith(Application.ModulesRoot, StringComparison.Ordinal))
            {
                var path = folder.Substring(Application.ModulesRoot.Length);
                var index = path.IndexOf('/');

                var name = index == -1 ? path : path.Substring(0, index);
                var assetPaths = _environment.GetModule(name).AssetPaths;

                var folders = new HashSet<string>(StringComparer.Ordinal);

                foreach (var assetPath in assetPaths.Where(a => a.StartsWith(folder, StringComparison.Ordinal)))
                {
                    path = assetPath.Substring(folder.Length + 1);
                    index = path.IndexOf('/');

                    if (index == -1)
                    {
                        entries.Add(GetFileInfo(assetPath));
                    }
                    else
                    {
                        folders.Add(path.Substring(0, index));
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

            var path = _contentRoot + NormalizePath(subpath);

            if (path.StartsWith(Application.ModulesRoot, StringComparison.Ordinal))
            {
                path = path.Substring(Application.ModulesRoot.Length);
                var index = path.IndexOf('/');

                if (index != -1)
                {
                    return _environment.GetModule(path.Substring(0, index))
                        .GetFileInfo(path.Substring(index + 1));
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
            return path.Replace('\\', '/').Trim('/');
        }
    }
}
