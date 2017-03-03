using System;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.Environment.Extensions.Loaders
{
    internal class CompositeExtensionLoader : IExtensionLoader
    {
        private readonly IExtensionLoader[] _extensionLoaders;
        public CompositeExtensionLoader(params IExtensionLoader[] extensionLoaders)
        {
            _extensionLoaders = extensionLoaders ?? new IExtensionLoader[0];
        }

        public CompositeExtensionLoader(IEnumerable<IExtensionLoader> extensionLoaders)
        {
            if (extensionLoaders == null)
            {
                throw new ArgumentNullException(nameof(extensionLoaders));
            }
            _extensionLoaders = extensionLoaders.ToArray();
        }

        public int Order
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public ExtensionEntry Load(IExtensionInfo extensionInfo)
        {
            foreach (var loader in _extensionLoaders.OrderBy(x => x.Order))
            {
                var entry = loader.Load(extensionInfo);
                if (entry != null)
                {
                    return entry;
                }
            }
            return new ExtensionEntry { ExtensionInfo = extensionInfo, IsError = true, ExportedTypes = Enumerable.Empty<Type>() };
        }
    }
}
