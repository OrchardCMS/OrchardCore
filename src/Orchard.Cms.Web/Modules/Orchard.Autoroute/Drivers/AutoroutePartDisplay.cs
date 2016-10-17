using System;
using System.Linq;
using System.Threading.Tasks;
using Orchard.Autoroute.Model;
using Orchard.Autoroute.Models;
using Orchard.Autoroute.ViewModels;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentManagement.MetaData;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;

namespace Orchard.Autoroute.Drivers
{
    public class AutoroutePartDisplay : ContentPartDisplayDriver<AutoroutePart>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public AutoroutePartDisplay(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public override IDisplayResult Edit(AutoroutePart autoroutePart)
        {
            return Shape<AutoroutePartViewModel>("AutoroutePart_Edit", model =>
            {
                model.Path = autoroutePart.Path;
                model.AutoroutePart = autoroutePart;

                var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(autoroutePart.ContentItem.ContentType);
                var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(x => String.Equals(x.PartDefinition.Name, nameof(AutoroutePart), StringComparison.Ordinal));
                model.Settings = contentTypePartDefinition.Settings.ToObject<AutoroutePartSettings>();

                return Task.CompletedTask;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(AutoroutePart model, IUpdateModel updater)
        {
            var result = await updater.TryUpdateModelAsync(model, Prefix, t => t.Path);

            return Edit(model);
        }
    }
}
