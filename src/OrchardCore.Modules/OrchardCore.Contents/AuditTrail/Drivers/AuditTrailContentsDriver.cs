using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.ViewModels;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Contents.AuditTrail.Drivers
{
    public class AuditTrailContentsDriver : ContentDisplayDriver
    {
        // TODO: What permission are we looking for here?
        public override IDisplayResult Display(ContentItem contentItem, IUpdateModel updater)
        {
            return Initialize<ContentItemViewModel>("AuditTrailContentsAction_SummaryAdmin", m => m.ContentItem = contentItem).Location("SummaryAdmin", "ActionsMenu:10");
        }
    }
}
