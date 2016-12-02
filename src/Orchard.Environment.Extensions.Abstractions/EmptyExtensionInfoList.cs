using Orchard.Environment.Extensions.Features;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace Orchard.Environment.Extensions
{
    public class EmptyExtensionInfoList : IExtensionInfoList
    {
        /// <summary>
        /// A shared instance of <see cref="EmptyExtensionInfoList"/>
        /// </summary>
        public static EmptyExtensionInfoList Singleton { get; } = new EmptyExtensionInfoList();

        public IExtensionInfo this[string key]
        {
            get
            {
                throw new System.IndexOutOfRangeException();
            }
        }

        public int Count => 0;

        public IEnumerable<IFeatureInfo> Features
        {
            get
            {
                return Enumerable.Empty<IFeatureInfo>();
            }
        }

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator to an empty collection.</returns>
        public IEnumerator<IExtensionInfo> GetEnumerator() => Enumerable.Empty<IExtensionInfo>().GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
