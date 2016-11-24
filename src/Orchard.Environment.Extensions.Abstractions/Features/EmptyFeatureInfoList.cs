using System.Collections;
using System.Collections.Generic;

namespace Orchard.Environment.Extensions.Features
{
    public class EmptyFeatureInfoList : IFeatureInfoList
    {
        private readonly IDictionary<string, IFeatureInfo> _emptyDictionary =
            new Dictionary<string, IFeatureInfo>();

        public IFeatureInfo this[int index]
        {
            get
            {
                throw new System.IndexOutOfRangeException();
            }
        }

        public IFeatureInfo this[string key]
        {
            get
            {
                return _emptyDictionary[key];
            }
        }

        public int Count => 0;

        public IExtensionInfoList Extensions
        {
            get
            {
                return new EmptyExtensionInfoList();
            }
        }

        public IEnumerator<IFeatureInfo> GetEnumerator()
        {
            return _emptyDictionary.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _emptyDictionary.Values.GetEnumerator();
        }
    }
}
