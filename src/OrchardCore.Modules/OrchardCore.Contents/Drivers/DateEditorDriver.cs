using System;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.Contents.Models;
using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Modules;

namespace OrchardCore.Contents.Drivers
{
    public class DateEditorDriver : ContentPartDisplayDriver<CommonPart>
    {
        private readonly ILocalClock _localClock;

        public DateEditorDriver(ILocalClock localClock)
        {
            _localClock = localClock;
        }

        public override IDisplayResult Edit(CommonPart part, BuildPartEditorContext context)
        {
            var settings = context.TypePartDefinition.GetSettings<CommonPartSettings>();

            if (settings.DisplayDateEditor)
            {
                return Initialize<DateEditorViewModel>("CommonPart_Edit__Date", async model =>
                {
                    model.LocalDateTime = part.ContentItem.CreatedUtc.HasValue
                    ? (DateTime?)(await _localClock.ConvertToLocalAsync(part.ContentItem.CreatedUtc.Value)).DateTime
                    : null;
                });
            }

            return null;
        }

        public override async Task<IDisplayResult> UpdateAsync(CommonPart part, IUpdateModel updater, UpdatePartEditorContext context)
        {
            var settings = context.TypePartDefinition.GetSettings<CommonPartSettings>();

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

            return Edit(part, context);
        }
    }
}
