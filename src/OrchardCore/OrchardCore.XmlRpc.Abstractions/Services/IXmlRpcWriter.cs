using System.Xml.Linq;
using OrchardCore.XmlRpc.Models;

namespace OrchardCore.XmlRpc.Services
{
    /// <summary>
    /// Abstraction to write XML based on rpc entities.
    /// </summary>
    public interface IXmlRpcWriter
    {
        /// <summary>
        /// Maps a method response to XML.
        /// </summary>
        /// <param name="rpcMethodResponse">The method response to be mapped.</param>
        /// <returns>The XML element.</returns>
        XElement MapMethodResponse(XRpcMethodResponse rpcMethodResponse);

        /// <summary>
        /// Maps rpc data to XML.
        /// </summary>
        /// <param name="rpcData">The rpc data.</param>
        /// <returns>The XML element.</returns>
        XElement MapData(XRpcData rpcData);

        /// <summary>
        /// Maps a rpc struct to XML.
        /// </summary>
        /// <param name="rpcStruct">The rpc struct.</param>
        /// <returns>The XML element.</returns>
        XElement MapStruct(XRpcStruct rpcStruct);

        /// <summary>
        /// Maps a rpc array to XML.
        /// </summary>
        /// <param name="rpcArray">The rpc array.</param>
        /// <returns>The XML element.</returns>
        XElement MapArray(XRpcArray rpcArray);
    }
}
