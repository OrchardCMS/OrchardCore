using System.Text.Encodings.Web;
using Orchard.ContentManagement;
using Orchard.Core.XmlRpc;
using Orchard.Core.XmlRpc.Models;
using Orchard.MetaWeblog;
using Orchard.Title.Model;

namespace Orchard.Title.RemotePublishing
{
    public class TitleMetaWeblogDriver : MetaWeblogDriver
    {
        private readonly HtmlEncoder _encoder;

        public TitleMetaWeblogDriver(HtmlEncoder encoder)
        {
            _encoder = encoder;
        }

        public override void BuildPost(XRpcStruct rpcStruct, XmlRpcContext context, ContentItem contentItem)
        {
            var titlePart = contentItem.As<TitlePart>();
            if (titlePart == null)
            {
                return;
            }

            rpcStruct.Set("title", _encoder.Encode(titlePart.Title));
        }

        public override void EditPost(XRpcStruct rpcStruct, ContentItem contentItem)
        {
            if (contentItem.As<TitlePart>() != null)
            {
                contentItem.Alter<TitlePart>(x => x.Title = rpcStruct.Optional<string>("title"));
            }
        }
    }
}
