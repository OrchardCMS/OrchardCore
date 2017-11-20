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
        private const string RootFolder = "Modules";
        private IHostingEnvironment _environment;
        private string _subPath;

        public ModuleEmbeddedFileProvider(IHostingEnvironment hostingEnvironment, string subPath = null)
        {
            _environment = hostingEnvironment;
            _subPath = subPath?.Replace('\\', '/').Trim('/') ?? "";
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            if (subpath == null)
            {
                return NotFoundDirectoryContents.Singleton;
            }

            var subPath = subpath.Replace('\\', '/').Trim('/');

            if (_subPath.Length > 0)
            {
                subPath = _subPath + '/' + subPath;
            }

            var entries = new List<IFileInfo>();

            if (subPath == "")
            {
                entries.Add(new EmbeddedDirectoryInfo(RootFolder));
            }
            else if (subPath == RootFolder)
            {
                entries.AddRange(_environment.GetModuleNames().Select(x => new EmbeddedDirectoryInfo(x)));
            }
            else if (subPath.StartsWith(RootFolder))
            {
                subPath = subPath.Substring(RootFolder.Length + 1);

                var index = subPath.IndexOf("/");
                var moduleId = index == -1 ? subPath : subPath.Substring(0, subPath.IndexOf("/"));

                var folders = new HashSet<string>();
                var paths = _environment.GetModuleAssets(moduleId);

                foreach (var path in paths.Where(x => x.StartsWith(RootFolder + '/' + subPath)))
                {
                    var trailingPath = path.Replace(RootFolder + '/' + subPath + '/', "");

                    if (trailingPath.IndexOf('/') == -1)
                    {
                        entries.Add(GetFileInfo(path));
                    }
                    else
                    {
                        folders.Add(trailingPath.Substring(0, trailingPath.IndexOf('/')));
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

            var subPath = subpath.Replace('\\', '/').Trim('/');

            if (_subPath.Length > 0)
            {
                subPath = _subPath + '/' + subPath;
            }

            if (subPath.StartsWith(RootFolder + '/'))
            {
                subPath = subPath.Substring(RootFolder.Length + 1);
                var index = subPath.IndexOf("/");

                if (index != -1)
                {
                    var moduleId = subPath.Substring(0, subPath.IndexOf("/"));
                    var fileName = subPath.Substring(moduleId.Length + 1);
                    var fileInfo = _environment.GetModuleFileInfo(moduleId, fileName);

                    if (fileInfo != null)
                    {
                        return fileInfo;
                    }
                }
            }

            return new NotFoundFileInfo(subpath);
        }

        public IChangeToken Watch(string filter)
        {
            return NullChangeToken.Singleton;
        }
    }
}
