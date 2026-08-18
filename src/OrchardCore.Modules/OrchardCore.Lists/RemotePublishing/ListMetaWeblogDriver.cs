using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Lists.Models;
using OrchardCore.Modules;

namespace OrchardCore.Lists.RemotePublishing;

[RequireFeatures("OrchardCore.RemotePublishing")]
public sealed class ListMetaWeblogDriver : ContentPartDisplayDriver<ListPart>
{
    public override IDisplayResult Display(ListPart listPart, BuildPartDisplayContext context)
    {
        return Dynamic("ListPart_RemotePublishing", static (shape, listPart) =>
        {
            shape.ContentItem = listPart.ContentItem;
        }, listPart).Location("Content");
    }
}
