using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using OrchardCore.Modules;
using OrchardCore.Modules.FileProviders;

namespace OrchardCore.Mvc
{
    /// <summary>
    /// This custom <see cref="IFileProvider"/> provides the virtual directory contents
    /// of the application '/Pages' folder when embedded in the precompiled '.Views.dll'.
    /// </summary>
    public class ApplicationCompiledPageFileProvider : IFileProvider
    {
        private static IList<string> _pages;
        private static object _synLock = new object();

        private readonly IHostingEnvironment _environment;

        public ApplicationCompiledPageFileProvider(IHostingEnvironment environment)
        {
            _environment = environment;

            if (_pages != null)
            {
                return;
            }

            lock (_synLock)
            {
                if (_pages == null)
                {
                    var application = environment.GetApplication();

                    var pages = new List<string>();

                    var precompiledAssemblyPath = Path.Combine(Path.GetDirectoryName(application.Assembly.Location),
                        application.Assembly.GetName().Name + ".Views.dll");

                    if (File.Exists(precompiledAssemblyPath))
                    {
                        try
                        {
                            var assembly = Assembly.LoadFile(precompiledAssemblyPath);
                            var provider = new CompiledRazorAssemblyPart(assembly) as IRazorCompiledItemProvider;

                            pages = provider.CompiledItems.Where(i => i.Identifier.StartsWith("/Pages/") &&
                                i.Kind == RazorPageDocumentClassifierPass.RazorPageDocumentKind)
                                .Select(i => i.Identifier.TrimStart('/')).ToList();
                        }

                        catch (FileLoadException)
                        {
                            // Don't throw if assembly cannot be loaded. This can happen if the file is not a managed assembly.
                        }
                    }

                    _pages = pages;
                }
            }
        }

        private Application Application => _environment.GetApplication();

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            if (subpath == null)
            {
                return NotFoundDirectoryContents.Singleton;
            }

            var folder = NormalizePath(subpath);

            var entries = new List<IFileInfo>();

            if (folder == Application.ModulePath)
            {
                entries.Add(new EmbeddedDirectoryInfo("Pages"));
            }
            else if (folder.StartsWith(Application.ModuleRoot + "Pages", StringComparison.Ordinal))
            {
                var folders = new HashSet<string>(StringComparer.Ordinal);
                var folderSlash = folder.Substring(Application.ModuleRoot.Length) + '/';

                foreach (var page in _pages.Where(a => a.StartsWith(folderSlash, StringComparison.Ordinal)))
                {
                    var folderPath = page.Substring(folderSlash.Length);
                    var pathIndex = folderPath.IndexOf('/');
                    var isFilePath = pathIndex == -1;

                    if (isFilePath)
                    {
                        entries.Add(new EmptyPageFileInfo(Path.GetFileName(page)));
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

    internal class EmptyPageFileInfo : IFileInfo
    {
        private static readonly byte[] _content = Encoding.UTF8.GetBytes('@' + PageDirective.Directive.Directive);

        public EmptyPageFileInfo(string name) { Name = name; }

        public bool Exists => true;
        public long Length { get { return _content.Length; } }
        public string PhysicalPath => null;
        public string Name { get; }
        public DateTimeOffset LastModified { get { return DateTimeOffset.MinValue; } }
        public bool IsDirectory => false;

        public Stream CreateReadStream() { return new MemoryStream(_content); }
    }
}
