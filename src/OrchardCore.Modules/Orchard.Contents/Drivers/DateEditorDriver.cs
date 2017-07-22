using System;
using System.Linq;
using System.Threading.Tasks;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentManagement.MetaData;
using Orchard.Contents.Models;
using Orchard.Contents.ViewModels;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;

namespace Orchard.Contents.Drivers
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
                return Shape<DateEditorViewModel>("CommonPart_Edit__Date", model =>
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