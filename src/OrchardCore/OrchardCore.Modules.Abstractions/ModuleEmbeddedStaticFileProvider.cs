using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Primitives;

namespace OrchardCore.Modules
{
    /// <summary>
    /// This custom <see cref="IFileProvider"/> implementation provides the file contents
    /// of embedded files in Module assemblies whose path is under a Module 'wwwroot' folder.
    /// </summary>
    public class ModuleEmbeddedStaticFileProvider : IFileProvider
    {
        private static IEnumerable<string> _modules;
        private static object _synLock = new object();

        private readonly IHostingEnvironment _environment;

        public ModuleEmbeddedStaticFileProvider(IHostingEnvironment environment)
        {
            _environment = environment;

            if (_modules != null)
            {
                return;
            }

            lock (_synLock)
            {
                if (_modules == null)
                {
                    _modules = _environment.GetApplication().ModuleNames;
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

            var index = path.IndexOf('/');

            if (index != -1)
            {
                var module = path.Substring(0, index);

                if (_modules.Contains(module))
                {
                    var fileSubPath = Module.WebRoot + path.Substring(index + 1);

                    if (module != _environment.ApplicationName)
                    {
                        return _environment.GetModule(module).GetFileInfo(fileSubPath);
                    }

                    fileSubPath = _environment.ContentRootPath + '/' + fileSubPath;
                    return new PhysicalFileInfo(new FileInfo(fileSubPath));
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
            return path.Replace('\\', '/').Trim('/').Replace("//", "/");
        }
    }
}