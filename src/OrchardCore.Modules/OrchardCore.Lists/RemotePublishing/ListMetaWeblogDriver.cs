using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Lists.Models;
using OrchardCore.Modules;

namespace OrchardCore.Lists.RemotePublishing
{
    [RequireFeatures("OrchardCore.RemotePublishing")]
    public class ListMetaWeblogDriver : ContentPartDisplayDriver<ListPart>
    {
        public override IDisplayResult Display(ListPart listPart, BuildPartDisplayContext context)
        {
            return Dynamic("ListPart_RemotePublishing", shape =>
            {
                shape.ContentItem = listPart.ContentItem;
            }).Location("Content");
        }
    }
}
