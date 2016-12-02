using Orchard.Environment.Extensions.Features;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.Environment.Extensions
{
    public class ExtensionInfoList : IExtensionInfoList
    {
        private readonly IList<IExtensionInfo> _extensions;

        public ExtensionInfoList(IList<IExtensionInfo> extensions) {
            _extensions = extensions;
        }

        public IExtensionInfo this[string key]
        {
            get { return _extensions.First(x => x.Id == key); }
        }

        private IEnumerable<IFeatureInfo> _features;

        public IEnumerable<IFeatureInfo> Features {
            get
            {
                if (_features == null)
                {
                    _features =
                        _extensions
                            .SelectMany(x => x.Features);
                }

                return _features;
            }
        }

        public IEnumerator<IExtensionInfo> GetEnumerator()
        {
            return _extensions.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _extensions.GetEnumerator();
        }
    }
}
