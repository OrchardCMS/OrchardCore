using System.Linq;
using System.Threading.Tasks;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentManagement.MetaData;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;
using Orchard.Alias.Models;
using Orchard.Alias.Settings;
using Orchard.Alias.ViewModels;

namespace Orchard.Alias.Drivers
{
    public class AliasPartDisplayDriver : ContentPartDisplayDriver<AliasPart>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public AliasPartDisplayDriver(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public override IDisplayResult Edit(AliasPart aliasPart)
        {
            return Shape<AliasPartViewModel>("AliasPart_Edit", m => BuildViewModel(m, aliasPart));
        }

        public override async Task<IDisplayResult> UpdateAsync(AliasPart model, IUpdateModel updater)
        {
            var settings = GetAliasPartSettings(model);

            await updater.TryUpdateModelAsync(model, Prefix, t => t.Alias);
            
            return Edit(model);
        }

        public AliasPartSettings GetAliasPartSettings(AliasPart part)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(part.ContentItem.ContentType);
            var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(p => p.PartDefinition.Name == nameof(AliasPart));
            var settings = contentTypePartDefinition.GetSettings<AliasPartSettings>();

            return settings;
        }

        private Task BuildViewModel(AliasPartViewModel model, AliasPart part)
        {
            var settings = GetAliasPartSettings(part);

            model.Alias = part.Alias;
            model.AliasPart = part;
            model.Settings = settings;

            return Task.CompletedTask;
        }
    }
}
