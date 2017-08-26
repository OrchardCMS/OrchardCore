using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
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
        private readonly ILiquidTemplateManager _liquidTemplatemanager;

        public LiquidPartDisplay(
            IContentDefinitionManager contentDefinitionManager,
            ILiquidTemplateManager liquidTemplatemanager)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _liquidTemplatemanager = liquidTemplatemanager;
        }

        public override IDisplayResult Display(LiquidPart liquidPart)
        {
            return Combine(
                Shape<LiquidPartViewModel>("LiquidPart", m => BuildViewModelAsync(m, liquidPart))
                    .Location("Detail", "Content:10"),
                Shape<LiquidPartViewModel>("LiquidPart_Summary", m => BuildViewModelAsync(m, liquidPart))
                    .Location("Summary", "Content:10")
            );
        }

        public override IDisplayResult Edit(LiquidPart liquidPart)
        {
            return Shape<LiquidPartViewModel>("LiquidPart_Edit", m => BuildViewModelAsync(m, liquidPart));
        }

        public override async Task<IDisplayResult> UpdateAsync(LiquidPart model, IUpdateModel updater)
        {
            await updater.TryUpdateModelAsync(model, Prefix, t => t.Liquid);

            return Edit(model);
        }

        private async Task BuildViewModelAsync(LiquidPartViewModel model, LiquidPart liquidPart)
        {
            var templateContext = new TemplateContext();
            templateContext.SetValue("ContentItem", liquidPart.ContentItem);
            templateContext.MemberAccessStrategy.Register<LiquidPartViewModel>();

            using (var writer = new StringWriter())
            {
                await _liquidTemplatemanager.RenderAsync(liquidPart.Liquid, writer, HtmlEncoder.Default, templateContext);
                model.Html = writer.ToString();
            }

            model.Liquid = liquidPart.Liquid;
            model.LiquidPart = liquidPart;
            model.ContentItem = liquidPart.ContentItem;
        }
    }
}
