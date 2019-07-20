using System;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Contents.Models;
using OrchardCore.Contents.ViewModels;
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

        public override async Task<IDisplayResult> EditAsync(CommonPart part, BuildPartEditorContext context)
        {
            var settings = await GetSettingsAsync(part);

            if (settings.DisplayDateEditor)
            {
                return Initialize<DateEditorViewModel>("CommonPart_Edit__Date", async model =>
                {
                    model.LocalDateTime = part.ContentItem.CreatedUtc.Value == null ? (DateTime?)null : (await _localClock.ConvertToLocalAsync(part.ContentItem.CreatedUtc.Value)).DateTime;
                });
            }

            return null;
        }

        public override async Task<IDisplayResult> UpdateAsync(CommonPart part, UpdatePartEditorContext context)
        {
            var settings = await GetSettingsAsync(part);

            if (settings.DisplayDateEditor)
            {
                var model = new DateEditorViewModel();
                await context.Updater.TryUpdateModelAsync(model, Prefix);

                if (model.LocalDateTime == null)
                {
                    part.ContentItem.CreatedUtc = null;
                }
                else
                {
                    part.ContentItem.CreatedUtc = await _localClock.ConvertToUtcAsync(model.LocalDateTime.Value);
                }
            }

            return await EditAsync(part, context);
        }

        public async Task<CommonPartSettings> GetSettingsAsync(CommonPart part)
        {
            var contentTypeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync(part.ContentItem.ContentType);
            var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(x => String.Equals(x.PartDefinition.Name, "CommonPart", StringComparison.Ordinal));
            return contentTypePartDefinition.GetSettings<CommonPartSettings>();
        }
    }
}
