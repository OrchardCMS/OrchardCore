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
            _paths = optionsAccessor.Value.Options.Select(o => '/' + o.SearchPath);
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            if (subpath == "/")
            {
                return _fileProvider.GetDirectoryContents(subpath);
            }

            foreach (var path in _paths)
            {
                if (subpath.StartsWith(path))
                {
                    if (subpath.Length == path.Length || subpath.Contains("/Pages") ||
                        subpath.Substring(path.Length + 1).IndexOf('/') == -1)
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
    }
}
