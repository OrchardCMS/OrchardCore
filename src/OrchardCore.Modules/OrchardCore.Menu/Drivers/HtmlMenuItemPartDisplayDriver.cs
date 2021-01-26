using System.Threading.Tasks;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Menu.Models;
using OrchardCore.Menu.ViewModels;

namespace OrchardCore.Menu.Drivers
{
    public class HtmlMenuItemPartDisplayDriver : ContentPartDisplayDriver<HtmlMenuItemPart>
    {

        public override IDisplayResult Display(HtmlMenuItemPart part, BuildPartDisplayContext context)
        {
            return Combine(
                Dynamic("HtmlMenuItemPart_Admin", shape =>
                {
                    shape.MenuItemPart = part;
                })
                .Location("Admin", "Content:10"),
                Dynamic("HtmlMenuItemPart_Thumbnail", shape =>
                {
                    shape.MenuItemPart = part;
                })
                .Location("Thumbnail", "Content:10")
            );
        }

        public override IDisplayResult Edit(HtmlMenuItemPart part)
        {
            return Initialize<HtmlMenuItemPartEditViewModel>("HtmlMenuItemPart_Edit", model =>
            {
                model.Name = part.Name;
                model.Url = part.Url;
                model.Html = part.Html;
                model.MenuItemPart = part;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(HtmlMenuItemPart part, IUpdateModel updater)
        {
            await updater.TryUpdateModelAsync(part, Prefix, x => x.Name, x => x.Url, x => x.Html);

            return Edit(part);
        }
    }
}
