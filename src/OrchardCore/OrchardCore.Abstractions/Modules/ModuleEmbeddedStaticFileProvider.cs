using System.IO;
using System.Linq;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Primitives;

namespace OrchardCore.Modules
{
    /// <summary>
    /// This custom <see cref="IFileProvider"/> implementation provides the file contents
    /// of embedded files in Module assemblies whose path is under a Module 'wwwroot' folder.
    /// </summary>
    public class ModuleEmbeddedStaticFileProvider : IModuleStaticFileProvider
    {
        private readonly IApplicationContext _applicationContext;

        public ModuleEmbeddedStaticFileProvider(IApplicationContext applicationContext) => _applicationContext = applicationContext;

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

            // "{ModuleId}/**/*.*".
            if (index != -1)
            {
                var application = _applicationContext.Application;

                // Resolve the module id.
                var module = path[..index];

                // Check if it is an existing module.
                if (application.Modules.Any(m => m.Name == module))
                {
                    // Resolve the embedded file subpath: "wwwroot/**/*.*"
                    var fileSubPath = Module.WebRoot + path[(index + 1)..];

                    if (module != application.Name)
                    {
                        // Get the embedded file info from the module assembly.
                        return application.GetModule(module).GetFileInfo(fileSubPath);
                    }

                    // Application static files can be still requested in a regular way "/**/*.*".
                    // Here, it's done through the Application's module "{ApplicationName}/**/*.*".
                    // But we still serve them from the same physical files "{WebRootPath}/**/*.*".
                    return new PhysicalFileInfo(new FileInfo(application.Root + fileSubPath));
                }
            }

            return new NotFoundFileInfo(subpath);
        }

        public IChangeToken Watch(string filter) => NullChangeToken.Singleton;

        private static string NormalizePath(string path) => path.Replace('\\', '/').Trim('/').Replace("//", "/");
    }
}
