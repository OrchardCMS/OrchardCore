using System.Linq;
using System.Threading.Tasks;
using Orchard.Markdown.Model;
using Orchard.Markdown.Settings;
using Orchard.Markdown.ViewModels;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentManagement.MetaData;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;

namespace Orchard.Markdown.Drivers
{
    public class MarkdownPartDisplay : ContentPartDisplayDriver<MarkdownPart>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public MarkdownPartDisplay(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public override IDisplayResult Display(MarkdownPart markdownPart)
        {
            return Combine(
                Shape<MarkdownPartViewModel>("MarkdownPart", m => BuildViewModel(m, markdownPart))
                    .Location("Detail", "Content:10"),
                Shape<MarkdownPartViewModel>("MarkdownPart_Summary", m => BuildViewModel(m, markdownPart))
                    .Location("Summary", "Content:10")
            );
        }

        public override IDisplayResult Edit(MarkdownPart markdownPart)
        {
            return Shape<MarkdownPartViewModel>("MarkdownPart_Edit", m => BuildViewModel(m, markdownPart));
        }

        public override async Task<IDisplayResult> UpdateAsync(MarkdownPart model, IUpdateModel updater)
        {
            await updater.TryUpdateModelAsync(model, Prefix, t => t.Markdown);

            return Edit(model);
        }

        private Task BuildViewModel(MarkdownPartViewModel model, MarkdownPart markdownPart)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(markdownPart.ContentItem.ContentType);
            var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(p => p.PartDefinition.Name == nameof(MarkdownPart));
            var settings = contentTypePartDefinition.GetSettings<MarkdownPartSettings>();

            model.Markdown = markdownPart.Markdown;
            model.MarkdownPart = markdownPart;
            model.TypePartSettings = settings;

            return Task.CompletedTask;
        }
    }
}
