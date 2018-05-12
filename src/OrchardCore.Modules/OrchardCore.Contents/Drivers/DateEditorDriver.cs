using System;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Contents.Models;
using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Contents.Drivers
{
    public class DateEditorDriver : ContentPartDisplayDriver<CommonPart>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public DateEditorDriver(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public override IDisplayResult Edit(CommonPart part)
        {
            var settings = GetSettings(part);

            if (settings.DisplayDateEditor)
            {
                return Initialize<DateEditorViewModel>("CommonPart_Edit__Date", model =>
                {
                    model.CreatedUtc = part.ContentItem.CreatedUtc;
                });
            }

            return null;
        }

        public override async Task<IDisplayResult> UpdateAsync(CommonPart part, IUpdateModel updater)
        {
            var settings = GetSettings(part);

            if (settings.DisplayDateEditor)
            {
                var model = new DateEditorViewModel();
                await updater.TryUpdateModelAsync(model, Prefix);

                part.ContentItem.CreatedUtc = model.CreatedUtc;
            }

            return Edit(part);
        }

        public CommonPartSettings GetSettings(CommonPart part)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(part.ContentItem.ContentType);
            var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(x => String.Equals(x.PartDefinition.Name, "CommonPart", StringComparison.Ordinal));
            return contentTypePartDefinition.GetSettings<CommonPartSettings>();
        }
    }
}