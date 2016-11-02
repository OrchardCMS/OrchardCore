using System;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.Environment.Extensions
{
    internal class CompositeExtensionProvider : IExtensionProvider
    {
        private readonly IExtensionProvider[] _extensionProviders;
        public CompositeExtensionProvider(params IExtensionProvider[] extensionProviders)
        {
            _extensionProviders = extensionProviders ?? new IExtensionProvider[0];
        }

        public CompositeExtensionProvider(IEnumerable<IExtensionProvider> extensionProviders)
        {
            if (extensionProviders == null)
            {
                throw new ArgumentNullException(nameof(extensionProviders));
            }
            _extensionProviders = extensionProviders.ToArray();
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
