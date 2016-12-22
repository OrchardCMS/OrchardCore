using System.Threading.Tasks;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentManagement.Display.Models;
using Orchard.DisplayManagement.Views;
using Orchard.Lists.Models;

namespace Orchard.Lists.Drivers
{
    public class ListPartFeedDisplayDriver : ContentPartDisplayDriver<ListPart>
    {
        public override IDisplayResult Display(ListPart listPart, BuildPartDisplayContext context)
        {
            return Shape("ListPart_Feed", shape =>
            {
                shape.ContentItem = listPart.ContentItem;

                return Task.CompletedTask;
            })
            .Location("Detail", "Content");
        }
    }
}
