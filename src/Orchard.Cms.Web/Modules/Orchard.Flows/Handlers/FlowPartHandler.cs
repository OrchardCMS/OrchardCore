using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Flows.Models;

namespace Orchard.Flows.Handlers
{
    public class FlowPartHandler : ContentPartHandler<FlowPart>
    {
        public override void Created(CreateContentContext context, FlowPart part)
        {
            foreach(var contentItem in part.Widgets)
            {
                // Update version number so that IsNew reflects the actual status
                contentItem.Number = 1;
            }

            // Reflect the changes back to the content item
            part.Apply();
        }
    }
}