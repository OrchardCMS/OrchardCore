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
        private const string Root = "Modules";
        private const string RootWithTrailingSlash = "Modules/";
        private IHostingEnvironment _environment;
        private string _contentPathWithTrailingSlash;

        public ModuleEmbeddedFileProvider(IHostingEnvironment hostingEnvironment, string contentPath = null)
        {
            _environment = hostingEnvironment;
            _contentPathWithTrailingSlash = contentPath != null ? NormalizePath(contentPath) + '/' : "/";
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            if (subpath == null)
            {
                return NotFoundDirectoryContents.Singleton;
            }

            var subPath = NormalizePath(subpath);

            if (_contentPathWithTrailingSlash.Length > 1)
            {
                subPath = _contentPathWithTrailingSlash + subPath;
            }

            var entries = new List<IFileInfo>();

            if (subPath == "")
            {
                entries.Add(new EmbeddedDirectoryInfo(Root));
            }
            else if (subPath == Root)
            {
                entries.AddRange(_environment.GetModuleNames().Select(x => new EmbeddedDirectoryInfo(x)));
            }
            else if (subPath.StartsWith(RootWithTrailingSlash))
            {
                subPath = subPath.Substring(RootWithTrailingSlash.Length);

                var index = subPath.IndexOf("/");
                var moduleId = index == -1 ? subPath : subPath.Substring(0, index);

                var folders = new HashSet<string>();
                var paths = _environment.GetModuleAssets(moduleId);

                foreach (var path in paths.Where(x => x.StartsWith(RootWithTrailingSlash + subPath)))
                {
                    var trailingPath = path.Substring(RootWithTrailingSlash.Length + subPath.Length + 1);
                    index = trailingPath.IndexOf('/');

                    if (index == -1)
                    {
                        entries.Add(GetFileInfo(path));
                    }
                    else
                    {
                        folders.Add(trailingPath.Substring(0, index));
                    }
                }

                entries.AddRange(folders.Select(x => new EmbeddedDirectoryInfo(x)));
            }

            return new EmbeddedDirectoryContents(entries);
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            if (subpath == null)
            {
                return new NotFoundFileInfo(subpath);
            }

            var subPath = NormalizePath(subpath);

            if (_contentPathWithTrailingSlash.Length > 1)
            {
                subPath = _contentPathWithTrailingSlash + subPath;
            }

            if (subPath.StartsWith(RootWithTrailingSlash))
            {
                subPath = subPath.Substring(RootWithTrailingSlash.Length);
                var index = subPath.IndexOf("/");

                if (index != -1)
                {
                    var moduleId = subPath.Substring(0, subPath.IndexOf("/"));
                    var paths = _environment.GetModuleAssets(moduleId);

                    if (paths.Contains(RootWithTrailingSlash + subPath))
                    {
                        var fileName = subPath.Substring(moduleId.Length + 1);
                        var fileInfo = _environment.GetModuleFileInfo(moduleId, fileName);

                        if (fileInfo != null)
                        {
                            return fileInfo;
                        }
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
            return path.Replace('\\', '/').Trim('/');
        }
    }
}
