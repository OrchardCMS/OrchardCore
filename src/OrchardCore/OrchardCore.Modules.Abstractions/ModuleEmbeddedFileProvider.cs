using System;
using System.Collections.Generic;
using System.IO;
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
        private readonly string _appModulePath;
        private readonly string _appModuleRoot;
        private readonly IFileProvider _appModuleFileProvider;
        private IHostingEnvironment _environment;
        private readonly string _contentRoot;

        public ModuleEmbeddedFileProvider(IHostingEnvironment hostingEnvironment, string contentPath = null)
        {
            _environment = hostingEnvironment;
            _contentRoot = contentPath != null ? NormalizePath(contentPath) + '/' : "";
            _appModuleFileProvider = new PhysicalFileProvider(_environment.ContentRootPath);
            _appModulePath = _environment.GetModule(_environment.ApplicationName).SubPath;
            _appModuleRoot = _environment.GetModule(_environment.ApplicationName).Root;
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            if (subpath.Contains("Pages"))
            {
                ;
            }

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
            else if (folder == _appModulePath)
            {
                return _appModuleFileProvider.GetDirectoryContents("");
            }
            else if (folder.StartsWith(_appModuleRoot, StringComparison.Ordinal))
            {
                var tokenizer = new StringTokenizer(folder, new char[] { '/' });
                if (tokenizer.Any(s => s == "Pages" || s == "Views" || s == Module.StaticFilePath))
                {
                    var folderSubPath = folder.Substring(_appModuleRoot.Length);
                    return _appModuleFileProvider.GetDirectoryContents(folderSubPath);
                }
            }
            else if (folder.StartsWith(Application.ModulesRoot, StringComparison.Ordinal))
            {
                var path = folder.Substring(Application.ModulesRoot.Length);
                var index = path.IndexOf('/');
                var name = index == -1 ? path : path.Substring(0, index);
                var assetPaths = _environment.GetModule(name).AssetPaths;
                var folders = new HashSet<string>(StringComparer.Ordinal);
                var folderSlash = folder + '/';

                foreach (var assetPath in assetPaths.Where(a => a.StartsWith(folderSlash, StringComparison.Ordinal)))
                {
                    var folderPath = assetPath.Substring(folderSlash.Length);
                    var pathIndex = folderPath.IndexOf('/');
                    var isFilePath = pathIndex == -1;

                    if (isFilePath)
                    {
                        entries.Add(GetFileInfo(assetPath));
                    }
                    else
                    {
                        folders.Add(folderPath.Substring(0, pathIndex));
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

            if (path.StartsWith(_appModuleRoot, StringComparison.Ordinal))
            {
                var fileSubPath = path.Substring(_appModuleRoot.Length);

                if (File.Exists(fileSubPath))
                {
                    return _appModuleFileProvider.GetFileInfo(fileSubPath);
                }
            }
            else if (path.StartsWith(Application.ModulesRoot, StringComparison.Ordinal))
            {
                path = path.Substring(Application.ModulesRoot.Length);
                var index = path.IndexOf('/');

                if (index != -1)
                {
                    var moduleName = path.Substring(0, index);
                    var fileSubPath = path.Substring(index + 1);
                    return _environment.GetModule(moduleName).GetFileInfo(fileSubPath);
                }
            }

            return new NotFoundFileInfo(subpath);
        }

        public IChangeToken Watch(string filter)
        {

            if (filter == null)
            {
                return NullChangeToken.Singleton;
            }

            var path = _contentRoot + NormalizePath(filter);

            if (path.StartsWith(_appModuleRoot, StringComparison.Ordinal))
            {
                var fileSubPath = path.Substring(_appModuleRoot.Length);

                if (File.Exists(fileSubPath))
                {
                    return _appModuleFileProvider.Watch(fileSubPath);
                }
            }

            return NullChangeToken.Singleton;
        }

        private string NormalizePath(string path)
        {
            return path.Replace('\\', '/').Trim('/').Replace("//", "/");
        }
    }
}
