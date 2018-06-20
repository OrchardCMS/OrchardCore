using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Fluid;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Liquid;
using OrchardCore.Markdown.Model;
using OrchardCore.Markdown.Settings;
using OrchardCore.Markdown.ViewModels;

namespace OrchardCore.Markdown.Drivers
{
    public class MarkdownBodyPartDisplay : ContentPartDisplayDriver<MarkdownBodyPart>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ILiquidTemplateManager _liquidTemplatemanager;

        public MarkdownBodyPartDisplay(
            IContentDefinitionManager contentDefinitionManager,
            ILiquidTemplateManager liquidTemplatemanager)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _liquidTemplatemanager = liquidTemplatemanager;
        }

        public override IDisplayResult Display(MarkdownBodyPart MarkdownBodyPart)
        {
            return Initialize<MarkdownBodyPartViewModel>("MarkdownBodyPart", m => BuildViewModel(m, MarkdownBodyPart))
                .Location("Detail", "Content:10")
                .Location("Summary", "Content:10");
        }

        public override IDisplayResult Edit(MarkdownBodyPart MarkdownBodyPart)
        {
            return Initialize<MarkdownBodyPartViewModel>("MarkdownBodyPart_Edit", m => BuildViewModel(m, MarkdownBodyPart));
        }

        public override async Task<IDisplayResult> UpdateAsync(MarkdownBodyPart model, IUpdateModel updater)
        {
            await updater.TryUpdateModelAsync(model, Prefix, t => t.Markdown);

            return Edit(model);
        }

        private async Task BuildViewModel(MarkdownBodyPartViewModel model, MarkdownBodyPart MarkdownBodyPart)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(MarkdownBodyPart.ContentItem.ContentType);
            var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(p => p.PartDefinition.Name == nameof(MarkdownBodyPart));
            var settings = contentTypePartDefinition.GetSettings<MarkdownBodyPartSettings>();

            var templateContext = new TemplateContext();
            templateContext.SetValue("ContentItem", MarkdownBodyPart.ContentItem);
            templateContext.MemberAccessStrategy.Register<MarkdownBodyPartViewModel>();

            using (var writer = new StringWriter())
            {
                await _liquidTemplatemanager.RenderAsync(MarkdownBodyPart.Markdown, writer, NullEncoder.Default, templateContext);
                model.Source = writer.ToString();
                model.Html = Markdig.Markdown.ToHtml(model.Source ?? "");
            }

            model.ContentItem = MarkdownBodyPart.ContentItem;
            model.MarkdownBodyPart = MarkdownBodyPart;
            model.TypePartSettings = settings;
        }
    }
}
