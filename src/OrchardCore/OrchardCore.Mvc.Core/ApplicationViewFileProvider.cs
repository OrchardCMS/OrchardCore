using System;
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
    /// This custom <see cref="IFileProvider"/> implementation provides the file contents of
    /// the Application's module view files while in a development or production environment.
    /// </summary>
    public class ApplicationViewFileProvider : IFileProvider
    {
        private readonly IApplicationContext _applicationContext;

        public ApplicationViewFileProvider(IApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        private Application Application => _applicationContext.Application;

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            // 'GetDirectoryContents()' is used to discover shapes templates and build fixed binding tables.
            // So the embedded file provider can always provide the structure under modules "Views" folders.
            // But application's module shapes are not embedded, so we need to serve the application "Views".

            // The razor view engine also uses 'GetDirectoryContents()' to find razor pages (not mvc views).
            // So here, we also need to serve the directory contents under the application "Pages" folder.

            // Note: This provider is also used in production where application views may not be precompiled.

            if (subpath == null)
            {
                return NotFoundDirectoryContents.Singleton;
            }

            var folder = NormalizePath(subpath);

            // Under "Areas/{ApplicationName}".
            if (folder == Application.ModulePath)
            {
                // Serve the contents from the file system.
                return new PhysicalDirectoryContents(Application.Path);
            }
            // Under "Areas/{ApplicationName}/**".
            else if (folder.StartsWith(Application.ModuleRoot, StringComparison.Ordinal))
            {
                // Check for a "Pages" or a "Views" segment.
                var tokenizer = new StringTokenizer(folder, new char[] { '/' });
                if (tokenizer.Any(s => s == "Pages" || s == "Views"))
                {
                    // Resolve the subpath relative to the application's module root.
                    var folderSubPath = folder[Application.ModuleRoot.Length..];

                    // And serve the contents from the physical application root folder.
                    return new PhysicalDirectoryContents(Application.Root + folderSubPath);
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

            // "Areas/{ApplicationName}/**/*.*".
            if (path.StartsWith(Application.ModuleRoot, StringComparison.Ordinal))
            {
                // Resolve the subpath relative to the application's module.
                var fileSubPath = path[Application.ModuleRoot.Length..];

                // And serve the file from the physical application root folder.
                return new PhysicalFileInfo(new FileInfo(Application.Root + fileSubPath));
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

            // "Areas/{ApplicationName}/**/*.*".
            if (path.StartsWith(Application.ModuleRoot, StringComparison.Ordinal))
            {
                // Resolve the subpath relative to the application's module.
                var fileSubPath = path[Application.ModuleRoot.Length..];

                // And watch the application file from the physical application root folder.
                return new PollingFileChangeToken(new FileInfo(Application.Root + fileSubPath));
            }

            return NullChangeToken.Singleton;
        }

        private static string NormalizePath(string path) => path.Replace('\\', '/').Trim('/');
    }
}
