using System.Threading.Tasks;
using System.Xml.Linq;

namespace Orchard.Core.XmlRpc
{
    public interface IXmlRpcHandler
    {
        void SetCapabilities(XElement element);
        Task ProcessAsync(XmlRpcContext context);
    }
}