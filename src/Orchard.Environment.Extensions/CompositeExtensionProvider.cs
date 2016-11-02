using System.Collections.Generic;

namespace Orchard.Environment.Extensions
{
    internal class CompositeExtensionProvider : IExtensionProvider
    {
        private readonly IEnumerable<IExtensionProvider> _extensionProviders;
        public CompositeExtensionProvider(IEnumerable<IExtensionProvider> extensionProviders)
        {
            _extensionProviders = extensionProviders;
        }

        public IExtensionInfo GetExtensionInfo(string subPath)
        {
            foreach (var provider in _extensionProviders)
            {
                var extensionInfo = provider.GetExtensionInfo(subPath);
                if (extensionInfo != null)
                {
                    return extensionInfo;
                }
            }
            return null;
        }
    }
}
