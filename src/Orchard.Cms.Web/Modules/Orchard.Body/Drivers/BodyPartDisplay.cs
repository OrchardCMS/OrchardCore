using System.Linq;
using System.Threading.Tasks;
using Orchard.Body.Model;
using Orchard.Body.Settings;
using Orchard.Body.ViewModels;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentManagement.MetaData;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;

namespace Orchard.Body.Drivers
{
    public class BodyPartDisplay : ContentPartDisplayDriver<BodyPart>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public BodyPartDisplay(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public override IDisplayResult Display(BodyPart bodyPart)
        {
            return Combine(
                Shape<BodyPartViewModel>("BodyPart", m => BuildViewModel(m, bodyPart))
                    .Location("Detail", "Content:5"),
                Shape<BodyPartViewModel>("BodyPart_Summary", m => BuildViewModel(m, bodyPart))
                    .Location("Summary", "Content:10")
            );
        }

        public override IDisplayResult Edit(BodyPart bodyPart)
        {
            return Shape<BodyPartViewModel>("BodyPart_Edit", m => BuildViewModel(m, bodyPart));
        }

        public override async Task<IDisplayResult> UpdateAsync(BodyPart model, IUpdateModel updater)
        {
            await updater.TryUpdateModelAsync(model, Prefix, t => t.Body);

            return Edit(model);
        }

        private Task BuildViewModel(BodyPartViewModel model, BodyPart bodyPart)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(bodyPart.ContentItem.ContentType);
            var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(p => p.PartDefinition.Name == nameof(BodyPart));
            var settings = contentTypePartDefinition.GetSettings<BodyPartSettings>();

            model.Body = bodyPart.Body;
            model.BodyPart = bodyPart;
            model.TypePartSettings = settings;

            return Task.CompletedTask;
        }
    }
}
