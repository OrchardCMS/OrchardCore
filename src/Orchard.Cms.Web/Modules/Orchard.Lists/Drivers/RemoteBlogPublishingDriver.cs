using System.Threading.Tasks;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentManagement.Display.Models;
using Orchard.DisplayManagement.Views;
using Orchard.Lists.Models;

namespace Orchard.Lists.Drivers
{
    [OrchardFeature("Orchard.RemotePublishing")]
    public class RemoteBlogPublishingDriver : ContentPartDisplayDriver<ListPart>
    {
        public override IDisplayResult Display(ListPart listPart, BuildPartDisplayContext context)
        {
            return Shape("ListPart_RemotePublishing", shape =>
            {
                shape.ContentItem = listPart.ContentItem;
                return Task.CompletedTask;
            }).Location("Content");
        }
    }
}