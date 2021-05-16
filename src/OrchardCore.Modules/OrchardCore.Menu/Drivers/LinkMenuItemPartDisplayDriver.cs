using System.Threading.Tasks;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Menu.Models;
using OrchardCore.Menu.ViewModels;

namespace OrchardCore.Menu.Drivers
{
    public class LinkMenuItemPartDisplayDriver : ContentPartDisplayDriver<LinkMenuItemPart>
    {

        public override IDisplayResult Display(LinkMenuItemPart part, BuildPartDisplayContext context)
        {
            return Combine(
                Dynamic("LinkMenuItemPart_Admin", shape =>
                {
                    shape.MenuItemPart = part;
                })
                .Location("Admin", "Content:10"),
                Dynamic("LinkMenuItemPart_Thumbnail", shape =>
                {
                    shape.MenuItemPart = part;
                })
                .Location("Thumbnail", "Content:10")
            );
        }

        public override IDisplayResult Edit(LinkMenuItemPart part)
        {
            return Initialize<LinkMenuItemPartEditViewModel>("LinkMenuItemPart_Edit", model =>
            {
                model.Name = part.ContentItem.DisplayText;
                model.Url = part.Url;
                model.MenuItemPart = part;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(LinkMenuItemPart part, IUpdateModel updater)
        {
            var model = new LinkMenuItemPartEditViewModel();

            if (await updater.TryUpdateModelAsync(model, Prefix))
            {
                part.Url = model.Url;
                part.ContentItem.DisplayText = model.Name;
// This code can be removed in a later release.
#pragma warning disable 0618
                part.Name = model.Name;
#pragma warning restore 0618
            }
            return Edit(part);
        }
    }
}
