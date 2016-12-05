using System;
using System.Threading.Tasks;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentManagement.MetaData;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;
using Orchard.Menu.Models;
using Orchard.Menu.ViewModels;

namespace Orchard.Lists.Drivers
{
    public class MenuPartDisplayDriver : ContentPartDisplayDriver<MenuPart>
    {
        private readonly IContentManager _contentManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public MenuPartDisplayDriver(
            IContentDefinitionManager contentDefinitionManager,
            IContentManager contentManager,
            IServiceProvider serviceProvider
            )
        {
            _contentDefinitionManager = contentDefinitionManager;
            _serviceProvider = serviceProvider;
            _contentManager = contentManager;
        }

        public override IDisplayResult Edit(MenuPart part)
        {
            return Shape<MenuPartEditViewModel>("MenuPart_Edit", model =>
            {
                model.MenuPart = part;
                return Task.CompletedTask;
            });
        }

        public override Task<IDisplayResult> UpdateAsync(MenuPart part, IUpdateModel updater)
        {
            return base.UpdateAsync(part, updater);
        }
    }
}