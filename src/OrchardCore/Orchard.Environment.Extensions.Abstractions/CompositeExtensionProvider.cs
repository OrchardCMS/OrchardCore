using System;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.Environment.Extensions
{
    public class CompositeExtensionProvider : IExtensionProvider
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

        public int Order { get { throw new NotSupportedException(); } }

        public IExtensionInfo GetExtensionInfo(IManifestInfo manifestInfo, string subPath)
        {
            foreach (var provider in _extensionProviders.OrderBy(ep => ep.Order))
            {
                var extensionInfo = provider.GetExtensionInfo(manifestInfo, subPath);
                if (extensionInfo != null)
                {
                    return extensionInfo;
                }
            }
            return new NotFoundExtensionInfo(subPath);
        }
    }
}
