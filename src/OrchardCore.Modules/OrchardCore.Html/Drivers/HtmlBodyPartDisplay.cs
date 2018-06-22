using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Fluid;
using OrchardCore.Html.Model;
using OrchardCore.Html.Settings;
using OrchardCore.Html.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Liquid;

namespace OrchardCore.Html.Drivers
{
    public class HtmlBodyPartDisplay : ContentPartDisplayDriver<HtmlBodyPart>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ILiquidTemplateManager _liquidTemplatemanager;

        public HtmlBodyPartDisplay(
            IContentDefinitionManager contentDefinitionManager,
            ILiquidTemplateManager liquidTemplatemanager)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _liquidTemplatemanager = liquidTemplatemanager;
        }

        public override IDisplayResult Display(HtmlBodyPart HtmlBodyPart)
        {
            return Initialize<HtmlBodyPartViewModel>("HtmlBodyPart", m => BuildViewModelAsync(m, HtmlBodyPart))
                .Location("Detail", "Content:5")
                .Location("Summary", "Content:10");
        }

        public override IDisplayResult Edit(HtmlBodyPart HtmlBodyPart)
        {
            return Initialize<HtmlBodyPartViewModel>("HtmlBodyPart_Edit", m => BuildViewModelAsync(m, HtmlBodyPart));
        }

        public override async Task<IDisplayResult> UpdateAsync(HtmlBodyPart model, IUpdateModel updater)
        {
            var viewModel = new HtmlBodyPartViewModel();

            await updater.TryUpdateModelAsync(viewModel, Prefix, t => t.Source);

            model.Html = viewModel.Source;

            return Edit(model);
        }

        private async Task BuildViewModelAsync(HtmlBodyPartViewModel model, HtmlBodyPart HtmlBodyPart)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(HtmlBodyPart.ContentItem.ContentType);
            var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(p => p.Name == nameof(HtmlBodyPart));
            var settings = contentTypePartDefinition.GetSettings<HtmlBodyPartSettings>();

            var templateContext = new TemplateContext();
            templateContext.SetValue("ContentItem", HtmlBodyPart.ContentItem);
            templateContext.MemberAccessStrategy.Register<HtmlBodyPartViewModel>();

            using (var writer = new StringWriter())
            {
                await _liquidTemplatemanager.RenderAsync(HtmlBodyPart.Html, writer, NullEncoder.Default, templateContext);
                model.Html = writer.ToString();
            }

            model.ContentItem = HtmlBodyPart.ContentItem;
            model.Source = HtmlBodyPart.Html;
            model.HtmlBodyPart = HtmlBodyPart;
            model.TypePartSettings = settings;
        }
    }
}
