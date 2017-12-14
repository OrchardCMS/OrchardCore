using System.Xml.Linq;
using OrchardCore.XmlRpc.Models;

namespace OrchardCore.XmlRpc.Services
{
    /// <summary>
    /// Abstraction to read XML and convert it to rpc entities.
    /// </summary>
    public interface IXmlRpcReader
    {
        /// <summary>
        /// Maps an XML element to a rpc method call.
        /// </summary>
        /// <param name="source">The XML element to be mapped.</param>
        /// <returns>The rpc method call.</returns>
        XRpcMethodCall MapToMethodCall(XElement source);

        /// <summary>
        /// Maps an XML element to rpc data.
        /// </summary>
        /// <param name="source">The XML element to be mapped.</param>
        /// <returns>The rpc data.</returns>
        XRpcData MapToData(XElement source);

        /// <summary>
        /// Maps an XML element to a rpc struct.
        /// </summary>
        /// <param name="source">The XML element to be mapped.</param>
        /// <returns>The rpc struct.</returns>
        XRpcStruct MapToStruct(XElement source);

        /// <summary>
        /// Maps an XML element to a rpc array.
        /// </summary>
        /// <param name="source">The XML element to be mapped.</param>
        /// <returns>The rpc array.</returns>
        XRpcArray MapToArray(XElement source);
    }
}
