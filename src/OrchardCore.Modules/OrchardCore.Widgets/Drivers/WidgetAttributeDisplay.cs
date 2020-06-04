using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Widgets.Models;
namespace OrchardCore.Widgets.Drivers
{
    public class WidgetAttributesDisplay : ContentDisplayDriver
    {
        public override IDisplayResult Edit(ContentItem model, IUpdateModel updater)
        {
            var attributes = model.As<WidgetAttributes>();

            if (attributes == null)
            {
                attributes = new WidgetAttributes();
            }

            return Initialize<WidgetAttributes>("WidgetAttributes_Edit", m =>
            {
                m.Attributes = attributes.Attributes;
            }).Location("WidgetSettings");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentItem contentItem, IUpdateModel updater)
        {
            var attributes = contentItem.As<WidgetAttributes>();

            if (attributes == null)
            {
                attributes = new WidgetAttributes();
            }

            await contentItem.AlterAsync<WidgetAttributes>(model => updater.TryUpdateModelAsync(model, Prefix));

            return Edit(contentItem, updater);
        }
    }
}
