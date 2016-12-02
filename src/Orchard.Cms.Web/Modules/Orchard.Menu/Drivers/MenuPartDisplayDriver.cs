using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Display;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentManagement.Display.Models;
using Orchard.ContentManagement.Metadata.Models;
using Orchard.ContentManagement.MetaData;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;
using Orchard.Menu.Models;
using Orchard.Menu.ViewModels;
using YesSql.Core.Services;

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

        public override IDisplayResult Display(MenuPart part, BuildPartDisplayContext context)
        {
            return Combine(
                Shape("MenuPart", shape =>
                {
                    shape.Foo = "Bar";
                    return Task.CompletedTask;
                })
                .Location("Detail", "Content:10")
            );
        }

        public override IDisplayResult Edit(MenuPart part)
        {
            return Shape<MenuPartEditViewModel>("MenuPart_Edit", model =>
            {
                model.MenuPart = part;
                model.Name = part.Name;
                return Task.CompletedTask;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(MenuPart part, IUpdateModel updater)
        {
            await updater.TryUpdateModelAsync(part, Prefix, x => x.Name);

            return Edit(part);
        }
    }
}