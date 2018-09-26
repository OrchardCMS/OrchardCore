using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using OrchardCore.Modules;
using OrchardCore.Modules.FileProviders;

namespace OrchardCore.Mvc
{
    /// <summary>
    /// This custom <see cref="IFileProvider"/> provides the virtual directory contents of the 
    /// application's module 'Pages' folder when embedded in the '{ApplicationName}.Views.dll'.
    /// </summary>
    public class ApplicationCompiledPageFileProvider : IFileProvider
    {
        private static IList<string> _pages;
        private static object _synLock = new object();

        private readonly IApplicationContext _applicationContext;

        public ApplicationCompiledPageFileProvider(IApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;

            if (_pages != null)
            {
                return;
            }

            lock (_synLock)
            {
                if (_pages == null)
                {
                    var application = _applicationContext.Application;

                    var pages = new List<string>();

                    // Resolve the application precompiled assembly path.
                    var precompiledAssemblyPath = Path.Combine(Path.GetDirectoryName(application.Assembly.Location),
                        application.Assembly.GetName().Name + ".Views.dll");

                    if (File.Exists(precompiledAssemblyPath))
                    {
                        try
                        {
                            // Resolve the application compiled item provider.
                            var assembly = Assembly.LoadFile(precompiledAssemblyPath);
                            var provider = new CompiledRazorAssemblyPart(assembly) as IRazorCompiledItemProvider;

                            // Get application page ids which are paths from the root with a leading slash.
                            // And remove the leading slash to get normalized paths e.g "Pages/Foo.cshtml".
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

        private Application Application => _applicationContext.Application;

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            if (subpath == null)
            {
                return NotFoundDirectoryContents.Singleton;
            }

            var folder = NormalizePath(subpath);

            var entries = new List<IFileInfo>();

            // Under "Areas/{ApplicationName}".
            if (folder == Application.ModulePath)
            {
                // Always add a "Pages" folder.
                entries.Add(new EmbeddedDirectoryInfo("Pages"));
            }
            // Under "Areas/{ApplicationName}/Pages" or "Areas/{ApplicationName}/Pages/**".
            else if (folder.StartsWith(Application.ModuleRoot + "Pages", StringComparison.Ordinal))
            {
                // Skip the "Areas/{ApplicationName}/" part from the given folder path.
                // So we get "Pages" or "Pages/**" paths relative to the application root.
                var subFolder = folder.Substring(Application.ModuleRoot.Length);

                // Resolve all files and folders directly under this subfolder by using the
                // compiled '_pages' paths which are also relative to the application root.
                NormalizedPaths.ResolveFolderContents(subFolder, _pages, out var files, out var folders);

                // And add them to the directory contents.
                entries.AddRange(files.Select(p => new EmptyPageFileInfo(Path.GetFileName(p))));
                entries.AddRange(folders.Select(n => new EmbeddedDirectoryInfo(n)));
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
}
