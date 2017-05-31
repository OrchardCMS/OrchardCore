using System.Collections;
using System.Collections.Generic;
using DotLiquid;
using Newtonsoft.Json.Linq;

namespace Orchard.Liquid
{
    /// <summary>
    /// Proxy for types not derived from DropBase
    /// </summary>
    public class JArrayDrop : Drop, IEnumerable<JToken>
    {
        private readonly JArray _proxiedObject;

        public JArrayDrop(JArray obj)
        {
            _proxiedObject = obj;
        }

        public IEnumerator<JToken> GetEnumerator()
        {
            return _proxiedObject.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _proxiedObject.GetEnumerator();
        }
    }
}
