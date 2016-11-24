using Orchard.Environment.Extensions.Features;
using System.Collections.Generic;
using System.Collections;

namespace Orchard.Environment.Extensions
{
    public class EmptyExtensionInfoList : IExtensionInfoList
    {
        private readonly IDictionary<string, IExtensionInfo> _emptyDictionary =
            new Dictionary<string, IExtensionInfo>();

        public IExtensionInfo this[int index]
        {
            get
            {
                throw new System.IndexOutOfRangeException();
            }
        }

        public IExtensionInfo this[string key]
        {
            get
            {
                return _emptyDictionary[key];
            }
        }

        public int Count => 0;

        public IFeatureInfoList Features
        {
            get
            {
                return new EmptyFeatureInfoList();
            }
        }

        public IEnumerator<IExtensionInfo> GetEnumerator()
        {
            return _emptyDictionary.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _emptyDictionary.Values.GetEnumerator();
        }
    }
}
