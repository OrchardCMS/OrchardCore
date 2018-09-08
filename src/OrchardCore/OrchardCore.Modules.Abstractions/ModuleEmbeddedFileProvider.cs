using System;
using System.Collections.Generic;
using System.Linq;
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
        private static object _synLock = new object();

        private readonly IApplicationContext _applicationContext;

        public ModuleEmbeddedFileProvider(IApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

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
                // Add the virtual folder ".Modules" containing all modules.
                entries.Add(new EmbeddedDirectoryInfo(Application.ModulesPath));
            }
            // Under ".Modules".
            else if (folder == Application.ModulesPath)
            {
                // Add virtual folders for all modules by using their assembly names (= module ids).
                entries.AddRange(Application.Modules.Select(m => new EmbeddedDirectoryInfo(m.Name)));
            }
            // Under ".Modules/{ModuleId}" or ".Modules/{ModuleId}/**".
            else if (folder.StartsWith(Application.ModulesRoot, StringComparison.Ordinal))
            {
                // Remove ".Modules/" from the folder path.
                var path = folder.Substring(Application.ModulesRoot.Length);
                var index = path.IndexOf('/');

                // Resolve the module id and get all its asset paths.
                var name = index == -1 ? path : path.Substring(0, index);
                var paths = Application.GetModule(name).AssetPaths;

                // Resolve all files and subfolders directly under this folder.
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

            // ".Modules/{ModuleId}/**/*.*".
            if (path.StartsWith(Application.ModulesRoot, StringComparison.Ordinal))
            {
                path = path.Substring(Application.ModulesRoot.Length);
                var index = path.IndexOf('/');

                if (index != -1)
                {
                    // Resolve the module id.
                    var module = path.Substring(0, index);

                    // Resolve the embedded file subpath: "**/*.*"
                    var fileSubPath = path.Substring(index + 1);

                    // Get the embedded file info from the module assembly.
                    return Application.GetModule(module).GetFileInfo(fileSubPath);
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