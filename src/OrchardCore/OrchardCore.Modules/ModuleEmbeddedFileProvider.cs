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
            _contentPathWithTrailingSlash = contentPath != null ? NormalizePath(contentPath) + '/' : "";
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            if (subpath == null)
            {
                return NotFoundDirectoryContents.Singleton;
            }

            var path = _contentPathWithTrailingSlash + NormalizePath(subpath);

            var entries = new List<IFileInfo>();

            if (path == "")
            {
                entries.Add(new EmbeddedDirectoryInfo(Root));
            }
            else if (path == Root)
            {
                entries.AddRange(_environment.GetModuleNames().Select(x => new EmbeddedDirectoryInfo(x)));
            }
            else if (path.StartsWith(RootWithTrailingSlash))
            {
                var underRootPath = path.Substring(RootWithTrailingSlash.Length);

                var index = underRootPath.IndexOf("/");
                var moduleId = index == -1 ? underRootPath : underRootPath.Substring(0, index);

                var folders = new HashSet<string>();
                var assets = _environment.GetModuleAssets(moduleId);

                foreach (var asset in assets.Where(x => x.StartsWith(path)))
                {
                    var underDirectoryPath = asset.Substring(path.Length + 1);
                    index = underDirectoryPath.IndexOf('/');

                    if (index == -1)
                    {
                        entries.Add(GetFileInfo(asset));
                    }
                    else
                    {
                        folders.Add(underDirectoryPath.Substring(0, index));
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

            var path = _contentPathWithTrailingSlash + NormalizePath(subpath);

            if (path.StartsWith(RootWithTrailingSlash))
            {
                var underRootPath = path.Substring(RootWithTrailingSlash.Length);
                var index = underRootPath.IndexOf("/");

                if (index != -1)
                {
                    var moduleId = underRootPath.Substring(0, index);
                    var assets = _environment.GetModuleAssets(moduleId);

                    if (assets.Contains(path))
                    {
                        var fileName = underRootPath.Substring(moduleId.Length + 1);
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
