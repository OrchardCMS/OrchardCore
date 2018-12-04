using System;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Contents.Models;
using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Modules;

namespace OrchardCore.Contents.Drivers
{
    public class DateEditorDriver : ContentPartDisplayDriver<CommonPart>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ILocalClock _localClock;

        public DateEditorDriver(IContentDefinitionManager contentDefinitionManager, ILocalClock localClock)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _localClock = localClock;
        }

        public override IDisplayResult Edit(CommonPart part)
        {
            var settings = GetSettings(part);

            if (settings.DisplayDateEditor)
            {
                return Initialize<DateEditorViewModel>("CommonPart_Edit__Date", async model =>
                {
                    model.LocalDateTime = part.ContentItem.CreatedUtc.Value == null ? (DateTime?)null : (await _localClock.ConvertToLocalAsync(part.ContentItem.CreatedUtc.Value)).DateTime;
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

                if (model.LocalDateTime == null)
                {
                    part.ContentItem.CreatedUtc = null;
                }
                else
                {
                    part.ContentItem.CreatedUtc = await _localClock.ConvertToUtcAsync(model.LocalDateTime.Value);
                }
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