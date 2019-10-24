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
    public class DateTimeOffsetFieldDisplayDriver : ContentFieldDisplayDriver<DateTimeOffsetField>
    {
        private readonly ILocalClock _localClock;

        public DateTimeOffsetFieldDisplayDriver(ILocalClock localClock)
        {
            _localClock = localClock;
        }

        public override IDisplayResult Display(DateTimeOffsetField field, BuildFieldDisplayContext context)
        {
            return Initialize<DisplayDateTimeOffsetFieldViewModel>(GetDisplayShapeType(context), async model =>
            {
                model.LocalDateTime = field.Value == null ? (DateTime?)null : (await _localClock.ConvertToLocalAsync(field.Value.Value)).DateTime;
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            })
            .Location("Content")
            .Location("SummaryAdmin", "");
        }

        public override IDisplayResult Edit(DateTimeOffsetField field, BuildFieldEditorContext context)
        {
            return Initialize<EditDateTimeOffsetFieldViewModel>(GetEditorShapeType(context), async model =>
            {
                model.LocalDateTime = field.Value == null ? (DateTime?)null : (await _localClock.ConvertToLocalAsync(field.Value.Value)).DateTime;
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(DateTimeOffsetField field, IUpdateModel updater, UpdateFieldEditorContext context)
        {
            var model = new EditDateTimeOffsetFieldViewModel();

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
