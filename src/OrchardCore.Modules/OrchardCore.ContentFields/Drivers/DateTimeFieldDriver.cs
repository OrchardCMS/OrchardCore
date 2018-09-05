using System;
using System.Threading.Tasks;
using OrchardCore.ContentFields.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Modules;

namespace OrchardCore.ContentFields.Fields
{
    public class DateTimeFieldDisplayDriver : ContentFieldDisplayDriver<DateTimeField>
    {
        private readonly ILocalClock _localClock;

        public DateTimeFieldDisplayDriver(ILocalClock localClock)
        {
            _localClock = localClock;
        }

        public override IDisplayResult Display(DateTimeField field, BuildFieldDisplayContext context)
        {
            return Initialize<DisplayDateTimeFieldViewModel>("DateTimeField", async model =>
            {
                model.LocalDateTime = field.Value == null ? (DateTime?)null : (await _localClock.ConvertToLocalAsync(field.Value.Value)).DateTime;
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            })
            .Location("Content")
            .Location("SummaryAdmin", "");
            ;
        }

        public override IDisplayResult Edit(DateTimeField field, BuildFieldEditorContext context)
        {
            return Initialize<EditDateTimeFieldViewModel>(GetEditorShapeType(context), async model =>
            {
                model.LocalDateTime = field.Value == null ? (DateTime?)null : (await _localClock.ConvertToLocalAsync(field.Value.Value)).DateTime;
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(DateTimeField field, IUpdateModel updater, UpdateFieldEditorContext context)
        {
            var model = new EditDateTimeFieldViewModel();
            
            if (await updater.TryUpdateModelAsync(model, Prefix, f => f.LocalDateTime))
            {
                if (model.LocalDateTime == null)
                {
                    field.Value = null;
                }
                else
                {
                    field.Value = await _localClock.ConvertToUtcAsync(model.LocalDateTime.Value);
                }  
            }

            return Edit(field, context);
        }
    }
}
