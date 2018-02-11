using System;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using OrchardCore.Modules;

namespace OrchardCore.Mvc.RazorPages
{
    class ModularPageRazorFileProvider : IFileProvider
    {
        private readonly IFileProvider _fileProvider;

        public ModularPageRazorFileProvider(IFileProvider fileProvider)
        {
            _fileProvider = fileProvider;
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            var folder = NormalizePath(subpath);

            if (folder == "")
            {
                return _fileProvider.GetDirectoryContents(subpath);
            }

            if (folder.StartsWith(Application.ModulesPath, StringComparison.Ordinal))
            {
                if (folder.Length == Application.ModulesPath.Length || folder.Contains("/Pages") ||
                    folder.Substring(Application.ModulesPath.Length + 1).IndexOf('/') == -1)
                {
                    return _fileProvider.GetDirectoryContents(subpath);
                }
            }

            return new NotFoundDirectoryContents();
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            return _fileProvider.GetFileInfo(subpath);
        }

        public IChangeToken Watch(string filter)
        {
            return _fileProvider.Watch(filter);
        }

        private string NormalizePath(string path)
        {
            return path.Replace('\\', '/').Trim('/');
        }
    }
}
