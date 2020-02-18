using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using OrchardCore.XmlRpc.Models;

namespace OrchardCore.XmlRpc.Services
{
    /// <summary>
    /// Abstraction to write XML based on rpc entities.
    /// </summary>
    public class XmlRpcWriter : IXmlRpcWriter
    {
        /// <summary>
        /// Provides the mapping function based on a type.
        /// </summary>
        private readonly IDictionary<Type, Func<XRpcData, XElement>> _dispatch;

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlRpcWriter"/> class.
        /// </summary>
        public XmlRpcWriter()
        {
            _dispatch = new Dictionary<Type, Func<XRpcData, XElement>>
                {
                    { typeof(int), p => new XElement("int", (int)p.Value) },
                    { typeof(bool), p => new XElement("boolean", (bool)p.Value ? "1" : "0") },
                    { typeof(string), p => new XElement("string", p.Value) },
                    { typeof(double), p => new XElement("double", (double)p.Value) },
                    { typeof(DateTime), p => new XElement("dateTime.iso8601", ((DateTime)p.Value).ToString("yyyyMMddTHH:mm:ssZ")) },
                    { typeof(DateTime?), p => new XElement("dateTime.iso8601", ((DateTime?)p.Value).Value.ToString("yyyyMMddTHH:mm:ssZ")) },
                    { typeof(DateTimeOffset), p => new XElement("dateTime.iso8601", ((DateTimeOffset)p.Value).ToString("yyyyMMddTHH:mm:ssZ")) },
                    { typeof(DateTimeOffset?), p => new XElement("dateTime.iso8601", ((DateTimeOffset?)p.Value).Value.ToString("yyyyMMddTHH:mm:ssZ")) },
                    { typeof(byte[]), p => new XElement("base64", Convert.ToBase64String((byte[])p.Value)) },
                    { typeof(XRpcStruct), p => MapStruct((XRpcStruct)p.Value) },
                    { typeof(XRpcArray), p => MapArray((XRpcArray)p.Value) },
                };
        }

        /// <summary>
        /// Maps a method response to XML.
        /// </summary>
        /// <param name="rpcMethodResponse">The method response to be mapped.</param>
        /// <returns>The XML element.</returns>
        public XElement MapMethodResponse(XRpcMethodResponse rpcMethodResponse)
        {
            if (rpcMethodResponse == null)
            {
                throw new ArgumentNullException(nameof(rpcMethodResponse));
            }

            // return a valid fault as per http://xmlrpc.scripting.com/spec.html
            if (rpcMethodResponse.Fault != null)
            {
                var members = new XRpcStruct();
                members.Set("faultCode", rpcMethodResponse.Fault.Code);
                members.Set("faultString", rpcMethodResponse.Fault.Message);

                return new XElement("methodResponse",
                    new XElement("fault",
                        new XElement("value", MapStruct(members))
                    )
                );
            }

            return new XElement("methodResponse",
                new XElement("params",
                    rpcMethodResponse.Params.Select(
                        p => new XElement("param", MapValue(p)))));
        }

        /// <summary>
        /// Maps rpc data to XML.
        /// </summary>
        /// <param name="rpcData">The rpc data.</param>
        /// <returns>The XML element.</returns>
        public XElement MapData(XRpcData rpcData)
        {
            if (rpcData == null)
            {
                throw new ArgumentNullException(nameof(rpcData));
            }

            return new XElement("param", MapValue(rpcData));
        }

        /// <summary>
        /// Maps a rpc struct to XML.
        /// </summary>
        /// <param name="rpcStruct">The rpc struct.</param>
        /// <returns>The XML element.</returns>
        public XElement MapStruct(XRpcStruct rpcStruct)
        {
            return new XElement(
                "struct",
                rpcStruct.Members.Select(
                    kv => new XElement(
                              "member",
                              new XElement("name", kv.Key),
                              MapValue(kv.Value))));
        }

        /// <summary>
        /// Maps a rpc array to XML.
        /// </summary>
        /// <param name="rpcArray">The rpc array.</param>
        /// <returns>The XML element.</returns>
        public XElement MapArray(XRpcArray rpcArray)
        {
            return new XElement(
                "array",
                new XElement(
                    "data",
                    rpcArray.Data.Select(MapValue)));
        }

        /// <summary>
        /// Maps rpc data to XML.
        /// </summary>
        /// <param name="rpcData">The rpc data.</param>
        /// <returns>The XML element.</returns>
        private XElement MapValue(XRpcData rpcData)
        {
            return new XElement("value", _dispatch[rpcData.Type](rpcData));
        }
    }
}
