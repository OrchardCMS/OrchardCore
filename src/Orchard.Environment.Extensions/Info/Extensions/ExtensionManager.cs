using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Orchard.Environment.Extensions.Info.Extensions
{
    public interface IExtensionManager
    {
    }

    public class ExtensionManager : IExtensionManager
    {
        private ExtensionOptions _extensionOptions;
        private IExtensionProvider _extensionProvider;
        private IHostingEnvironment _hostingEnvironment;

        public ExtensionManager(
            IOptions<ExtensionOptions> optionsAccessor,
            IExtensionProvider extensionProvider,
            IHostingEnvironment hostingEnvironment)
        {
            _extensionOptions = optionsAccessor.Value;
            _extensionProvider = extensionProvider;
            _hostingEnvironment = hostingEnvironment;
        }
        public IExtensionInfo GetExtension(string extensionId)
        {
            foreach (var searchPath in _extensionOptions.SearchPaths)
            {
                var subPath = 
                    Path.Combine(searchPath, extensionId);
                var extensionInfo = 
                    _extensionProvider.GetExtensionInfo(subPath);

                if (extensionInfo != null)
                {
                    return extensionInfo;
                }
            }

            return null;
        }

        public IEnumerable<IExtensionInfo> GetExtensions()
        {
            // (ngm) throw this to a static, no need to build this everytime
            IDictionary<string, IExtensionInfo> extensionsById
                = new Dictionary<string, IExtensionInfo>();

            foreach (var searchPath in _extensionOptions.SearchPaths)
            {
                foreach (var subDirectory in _hostingEnvironment
                    .ContentRootFileProvider
                    .GetDirectoryContents(searchPath).Where(x => x.IsDirectory))
                {
                    var extensionId = subDirectory.Name;
                    if (!extensionsById.ContainsKey(extensionId))
                    {
                        var subPath = Path.Combine(searchPath, extensionId);

                        var extensionInfo =
                            _extensionProvider.GetExtensionInfo(subPath);

                        if (extensionInfo != null)
                        {
                            extensionsById.Add(extensionId, extensionInfo);
                        }
                    }
                }
            }

            return extensionsById.Select(e => e.Value);
        }
    }
}
