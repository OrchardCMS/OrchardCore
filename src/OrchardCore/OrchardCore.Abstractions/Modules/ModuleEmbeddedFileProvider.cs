using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.FileProviders;
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
        private readonly IApplicationContext _applicationContext;

        public ModuleEmbeddedFileProvider(IApplicationContext applicationContext) => _applicationContext = applicationContext;

        private Application Application => _applicationContext.Application;

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            if (subpath == null)
            {
                return NotFoundDirectoryContents.Singleton;
            }

            var folder = NormalizePath(subpath);

            var entries = new List<IFileInfo>();

            // Under the root.
            if (folder == "")
            {
                // Add the virtual folder "Areas" containing all modules.
                entries.Add(new EmbeddedDirectoryInfo(Application.ModulesPath));
            }
            // Under "Areas".
            else if (folder == Application.ModulesPath)
            {
                // Add virtual folders for all modules by using their assembly names (module ids).
                entries.AddRange(Application.Modules.Select(m => new EmbeddedDirectoryInfo(m.Name)));
            }
            // Under "Areas/{ModuleId}" or "Areas/{ModuleId}/**".
            else if (folder.StartsWith(Application.ModulesRoot, StringComparison.Ordinal))
            {
                // Skip "Areas/" from the folder path.
                var path = folder[Application.ModulesRoot.Length..];
                var index = path.IndexOf('/');

                // Resolve the module id and get all its asset paths.
                var name = index == -1 ? path : path[..index];
                var paths = Application.GetModule(name).AssetPaths;

                // Resolve all files and folders directly under this given folder.
                NormalizedPaths.ResolveFolderContents(folder, paths, out var files, out var folders);

                // And add them to the directory contents.
                entries.AddRange(files.Select(p => GetFileInfo(p)));
                entries.AddRange(folders.Select(n => new EmbeddedDirectoryInfo(n)));
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

            // "Areas/**/*.*".
            if (path.StartsWith(Application.ModulesRoot, StringComparison.Ordinal))
            {
                // Skip the "Areas/" root.
                path = path[Application.ModulesRoot.Length..];
                var index = path.IndexOf('/');

                // "{ModuleId}/**/*.*".
                if (index != -1)
                {
                    // Resolve the module id.
                    var module = path[..index];

                    // Skip the module id to resolve the subpath.
                    var fileSubPath = path[(index + 1)..];

                    // If it is the app's module.
                    if (module == Application.Name)
                    {
                        // Serve the file from the physical application root folder.
                        return new PhysicalFileInfo(new FileInfo(Application.Root + fileSubPath));
                    }

                    // Get the embedded file info from the module assembly.
                    return Application.GetModule(module).GetFileInfo(fileSubPath);
                }
            }

            return new NotFoundFileInfo(subpath);
        }

        public IChangeToken Watch(string filter) => NullChangeToken.Singleton;

        private static string NormalizePath(string path) => path.Replace('\\', '/').Trim('/').Replace("//", "/");
    }
}
