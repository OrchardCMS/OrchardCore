using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using OrchardCore.Environment.Extensions;

namespace OrchardCore.Mvc.RazorPages
{
    class ModularPageRazorFileProvider : IFileProvider
    {
        private readonly IFileProvider _fileProvider;
        private readonly IEnumerable<string> _paths;

        public ModularPageRazorFileProvider(IFileProvider fileProvider, IOptions<ExtensionExpanderOptions> optionsAccessor)
        {
            _fileProvider = fileProvider;
            _paths = optionsAccessor.Value.Options.Select(o => o.SearchPath);
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            var folder = NormalizePath(subpath);

            if (folder == "")
            {
                return _fileProvider.GetDirectoryContents(subpath);
            }

            foreach (var path in _paths)
            {
                if (folder == path)
                {
                    return _fileProvider.GetDirectoryContents(subpath);
                }

                if (folder.StartsWith(path + '/', StringComparison.Ordinal))
                {
                    if (folder.Substring(path.Length + 1).IndexOf('/') == -1)
                    {
                        return _fileProvider.GetDirectoryContents(subpath);
                    }

                    var tokenizer = new StringTokenizer(folder, new char[] { '/' });
                    if (tokenizer.Any(s => s == "Pages" || s == "Components"))
                    {
                        return _fileProvider.GetDirectoryContents(subpath);
                    }

                    break;
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
