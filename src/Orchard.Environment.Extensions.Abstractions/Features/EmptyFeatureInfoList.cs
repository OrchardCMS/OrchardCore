using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.Environment.Extensions.Features
{
    public class EmptyFeatureInfoList : IFeatureInfoList
    {
        public IFeatureInfo this[int index]
        {
            get
            {
                throw new IndexOutOfRangeException();
            }
        }

        public IFeatureInfo this[string key]
        {
            get
            {
                throw new IndexOutOfRangeException();
            }
        }

        public int Count
        {
            get
            {
                return 0;
            }
        }

        public IExtensionInfoList Extensions
        {
            get
            {
                return null;
            }
        }

        public IEnumerator<IFeatureInfo> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
