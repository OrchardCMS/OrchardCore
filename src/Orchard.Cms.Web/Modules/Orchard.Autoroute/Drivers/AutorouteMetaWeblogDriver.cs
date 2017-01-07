using Orchard.ContentManagement;
using Orchard.Core.XmlRpc;
using Orchard.Core.XmlRpc.Models;
using Orchard.MetaWeblog;
using Orchard.Autoroute.Model;

namespace Orchard.Autoroute.Drivers
{
    public class AutorouteMetaWeblogDriver : MetaWeblogDriver
    {
        public override void BuildPost(XRpcStruct rpcStruct, XmlRpcContext context, ContentItem contentItem)
        {
            var autoroutePart = contentItem.As<AutoroutePart>();
            if (autoroutePart == null)
            {
                return;
            }

            rpcStruct.Set("wp_slug", autoroutePart.Path);
        }

        public override void EditPost(XRpcStruct rpcStruct, ContentItem contentItem)
        {
            if (contentItem.As<AutoroutePart>() != null)
            {
                contentItem.Alter<AutoroutePart>(x => x.Path = rpcStruct.Optional<string>("wp_slug"));
            }
        }
    }
}
