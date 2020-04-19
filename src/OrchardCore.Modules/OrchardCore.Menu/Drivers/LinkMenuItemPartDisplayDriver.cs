using System.Threading.Tasks;
using OrchardCore.ContentManagement;
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
        private readonly IContentManager _contentManager;

        public LinkMenuItemPartDisplayDriver(IContentManager contentManager)
        {
            _contentManager = contentManager;
        }

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
                model.Name = part.Name;
                model.Url = part.Url;
                model.MenuItemPart = part;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(LinkMenuItemPart part, IUpdateModel updater)
        {
            await updater.TryUpdateModelAsync(part, Prefix, x => x.Name, x => x.Url);

            return Edit(part);
        }
    }
}
