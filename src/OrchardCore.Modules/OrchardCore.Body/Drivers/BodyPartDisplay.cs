using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Fluid;
using OrchardCore.Body.Model;
using OrchardCore.Body.Settings;
using OrchardCore.Body.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Liquid;

namespace OrchardCore.Body.Drivers
{
    public class BodyPartDisplay : ContentPartDisplayDriver<BodyPart>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ILiquidTemplateManager _liquidTemplatemanager;

        public BodyPartDisplay(
            IContentDefinitionManager contentDefinitionManager,
            ILiquidTemplateManager liquidTemplatemanager)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _liquidTemplatemanager = liquidTemplatemanager;
        }

        public override IDisplayResult Display(BodyPart bodyPart)
        {
            return Initialize<BodyPartViewModel>("BodyPart", m => BuildViewModelAsync(m, bodyPart))
                .Location("Detail", "Content:5")
                .Location("Summary", "Content:10");
        }

        public override IDisplayResult Edit(BodyPart bodyPart)
        {
            return Initialize<BodyPartViewModel>("BodyPart_Edit", m => BuildViewModelAsync(m, bodyPart));
        }

        public override async Task<IDisplayResult> UpdateAsync(BodyPart model, IUpdateModel updater)
        {
            await updater.TryUpdateModelAsync(model, Prefix, t => t.Body);

            return Edit(model);
        }

        private async Task BuildViewModelAsync(BodyPartViewModel model, BodyPart bodyPart)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(bodyPart.ContentItem.ContentType);
            var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(p => p.Name == nameof(BodyPart));
            var settings = contentTypePartDefinition.GetSettings<BodyPartSettings>();

            var templateContext = new TemplateContext();
            templateContext.SetValue("ContentItem", bodyPart.ContentItem);
            templateContext.MemberAccessStrategy.Register<BodyPartViewModel>();

            using (var writer = new StringWriter())
            {
                await _liquidTemplatemanager.RenderAsync(bodyPart.Body, writer, NullEncoder.Default, templateContext);
                model.Html = writer.ToString();
            }

            model.ContentItem = bodyPart.ContentItem;
            model.Body = bodyPart.Body;
            model.BodyPart = bodyPart;
            model.TypePartSettings = settings;
        }
    }
}
