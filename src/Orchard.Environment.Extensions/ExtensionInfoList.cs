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

        private IFeatureInfoList _features;

        public IFeatureInfoList Features {
            get
            {
                if (_features == null)
                {
                    _features = new FeatureInfoList(
                        _extensions
                            .SelectMany(x => x.Features)
                            .ToList());
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
