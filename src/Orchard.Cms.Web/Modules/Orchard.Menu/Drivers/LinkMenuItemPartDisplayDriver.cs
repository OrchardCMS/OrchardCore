using System.Threading.Tasks;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentManagement.Display.Models;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;
using Orchard.Menu.Models;
using Orchard.Menu.ViewModels;

namespace Orchard.Lists.Drivers
{
    public class LinkMenuItemPartDisplayDriver : ContentPartDisplayDriver<LinkMenuItemPart>
    {
        private readonly IContentManager _contentManager;

        public LinkMenuItemPartDisplayDriver(
            IContentManager contentManager
            )
        {
            _contentManager = contentManager;
        }

        public override IDisplayResult Display(LinkMenuItemPart part, BuildPartDisplayContext context)
        {
            return Combine(
                Shape("LinkMenuItemPart_Admin", shape =>
                {
                    shape.MenuItemPart = part;
                    return Task.CompletedTask;
                })
                .Location("Admin", "Content:10"),
                Shape("LinkMenuItemPart_Thumbnail", shape =>
                {
                    shape.MenuItemPart = part;
                    return Task.CompletedTask;
                })
                .Location("Thumbnail", "Content:10")
            );
        }

        public override IDisplayResult Edit(LinkMenuItemPart part)
        {
            return Shape<LinkMenuItemPartEditViewModel>("LinkMenuItemPart_Edit", model =>
            {
                model.Name = part.Name;
                model.Url = part.Url;
                model.MenuItemPart = part;
                return Task.CompletedTask;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(LinkMenuItemPart part, IUpdateModel updater)
        {
            await updater.TryUpdateModelAsync(part, Prefix, x => x.Name, x => x.Url);

            return Edit(part);
        }
    }
}