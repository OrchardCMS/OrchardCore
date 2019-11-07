using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Title.Models;
using OrchardCore.Title.ViewModels;

namespace OrchardCore.Title.Drivers
{
    public class TitlePartDisplay : ContentPartDisplayDriver<TitlePart>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public TitlePartDisplay(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public override IDisplayResult Display(TitlePart titlePart)
        {
            return Initialize<TitlePartViewModel>("TitlePart", model =>
            {
                model.Title = titlePart.ContentItem.DisplayText;
                model.TitlePart = titlePart;
            })
            .Location("Detail", "Header:5")
            .Location("Summary", "Header:5");
        }

        public override IDisplayResult Edit(TitlePart titlePart)
        {
            return Initialize<TitlePartViewModel>("TitlePart_Edit", model =>
            {
                model.Title = titlePart.ContentItem.DisplayText;
                model.TitlePart = titlePart;
                model.Settings = GetSettings(titlePart);
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(TitlePart model, IUpdateModel updater)
        {
            await updater.TryUpdateModelAsync(model, Prefix, t => t.Title);

            model.ContentItem.DisplayText = model.Title;

            return Edit(model);
        }

        private TitlePartSettings GetSettings(TitlePart titlePart)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(titlePart.ContentItem.ContentType);
            var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(x => string.Equals(x.PartDefinition.Name, nameof(TitlePart)));
            return contentTypePartDefinition?.GetSettings<TitlePartSettings>();
        }
    }
}
