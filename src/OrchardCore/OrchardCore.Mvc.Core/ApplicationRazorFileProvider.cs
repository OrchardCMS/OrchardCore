using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Internal;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Primitives;

namespace OrchardCore.Modules
{
    /// <summary>
    /// This custom <see cref="IFileProvider"/> implementation provides the file contents
    /// of the Application Razor files while in a development or a production environment.
    /// </summary>
    public class ApplicationRazorFileProvider : IFileProvider
    {
        private static IFileProvider _pageFileProvider;
        private static object _synLock = new object();

        private readonly IApplicationContext _applicationContext;

        public ApplicationRazorFileProvider(IApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;

            if (_pageFileProvider != null)
            {
                return;
            }

            lock (_synLock)
            {
                if (_pageFileProvider == null)
                {
                    _pageFileProvider = new PhysicalFileProvider(Application.Root);
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

            if (folder.StartsWith(Application.ModuleRoot, StringComparison.Ordinal))
            {
                var tokenizer = new StringTokenizer(folder, new char[] { '/' });
                if (tokenizer.Any(s => s == "Pages" || s == "Views"))
                {
                    var folderSubPath = folder.Substring(Application.ModuleRoot.Length);
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

            if (path.StartsWith(Application.ModuleRoot, StringComparison.Ordinal))
            {
                var fileSubPath = path.Substring(Application.ModuleRoot.Length);
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

            if (path.StartsWith(Application.ModuleRoot, StringComparison.Ordinal))
            {
                var fileSubPath = path.Substring(Application.ModuleRoot.Length);
                return new PollingFileChangeToken(new FileInfo(Application.Root + fileSubPath));
            }

            if (path.Equals("**/*.cshtml"))
            {
                return _pageFileProvider.Watch("Pages/**/*.cshtml");
            }

            return NullChangeToken.Singleton;
        }

        private string NormalizePath(string path)
        {
            return path.Replace('\\', '/').Trim('/');
        }
    }
}