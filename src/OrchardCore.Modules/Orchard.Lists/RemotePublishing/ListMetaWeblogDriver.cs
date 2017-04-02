using System.Threading.Tasks;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentManagement.Display.Models;
using Orchard.DisplayManagement.Views;
using Orchard.Environment.Extensions.Features.Attributes;
using Orchard.Lists.Models;

namespace Orchard.Lists.RemotePublishing
{
    [OrchardFeature("Orchard.RemotePublishing")]
    public class ListMetaWeblogDriver : ContentPartDisplayDriver<ListPart>
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