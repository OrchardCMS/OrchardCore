using System.Threading.Tasks;
using System.Xml.Linq;

namespace OrchardCore.XmlRpc
{
    public interface IXmlRpcHandler
    {
        void SetCapabilities(XElement element);
        Task ProcessAsync(XmlRpcContext context);
    }
}
