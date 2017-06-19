using System.Threading.Tasks;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentManagement.MetaData;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;
using Orchard.Liquid.Model;
using Orchard.Liquid.ViewModels;

namespace Orchard.Liquid.Drivers
{
    public class LiquidPartDisplay : ContentPartDisplayDriver<LiquidPart>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public LiquidPartDisplay(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public override IDisplayResult Display(LiquidPart liquidPart)
        {
            return Combine(
                Shape<LiquidPartViewModel>("LiquidPart", m => BuildViewModel(m, liquidPart))
                    .Location("Detail", "Content:10"),
                Shape<LiquidPartViewModel>("LiquidPart_Summary", m => BuildViewModel(m, liquidPart))
                    .Location("Summary", "Content:10")
            );
        }

        public override IDisplayResult Edit(LiquidPart liquidPart)
        {
            return Shape<LiquidPartViewModel>("LiquidPart_Edit", m => BuildViewModel(m, liquidPart));
        }

        public override async Task<IDisplayResult> UpdateAsync(LiquidPart model, IUpdateModel updater)
        {
            await updater.TryUpdateModelAsync(model, Prefix, t => t.Liquid);

            return Edit(model);
        }

        private void BuildViewModel(LiquidPartViewModel model, LiquidPart liquidPart)
        {
            model.Liquid = liquidPart.Liquid;
            model.LiquidPart = liquidPart;
        }
    }
}
