using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.Environment.Extensions.Features
{
    public class EmptyFeatureInfoList : IFeatureInfoList
    {
        /// <summary>
        /// A shared instance of <see cref="EmptyFeatureInfoList"/> 
        /// </summary>
        public static EmptyFeatureInfoList Singleton { get; } = new EmptyFeatureInfoList();

        public IFeatureInfo this[string key]
        {
            get
            {
                throw new System.IndexOutOfRangeException();
            }
        }

        public int Count => 0;

        public IExtensionInfoList Extensions
        {
            get
            {
                return EmptyExtensionInfoList.Singleton;
            }
        }

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator to an empty collection.</returns>
        public IEnumerator<IFeatureInfo> GetEnumerator() => Enumerable.Empty<IFeatureInfo>().GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
