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
        private const string ModulesRoot = ".Modules/";
        private const string ModulesFolder = ".Modules";

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
                entries.Add(new EmbeddedDirectoryInfo(ModulesFolder));
            }
            else if (folder == ModulesFolder)
            {
                entries.AddRange(_environment.GetApplication().ModuleNames
                    .Select(n => new EmbeddedDirectoryInfo(n)));
            }
            else if (folder.StartsWith(ModulesRoot, StringComparison.Ordinal))
            {
                var subPath = folder.Substring(ModulesRoot.Length);

                var index = subPath.IndexOf('/');
                var name = index == -1 ? subPath : subPath.Substring(0, index);

                var directories = new HashSet<string>();
                var paths = _environment.GetModule(name).AssetPaths;

                foreach (var path in paths.Where(p => p.StartsWith(folder, StringComparison.Ordinal)))
                {
                    subPath = path.Substring(folder.Length + 1);
                    index = subPath.IndexOf('/');

                    if (index == -1)
                    {
                        entries.Add(GetFileInfo(path));
                    }
                    else
                    {
                        directories.Add(subPath.Substring(0, index));
                    }
                }

                entries.AddRange(directories.Select(d => new EmbeddedDirectoryInfo(d)));
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

            if (path.StartsWith(ModulesRoot, StringComparison.Ordinal))
            {
                var subPath = path.Substring(ModulesRoot.Length);
                var index = subPath.IndexOf('/');

                if (index != -1)
                {
                    var name = subPath.Substring(0, index);
                    var module = _environment.GetModule(name);
                    var fileName = subPath.Substring(name.Length + 1);
                    return module.GetFileInfo(fileName);
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
