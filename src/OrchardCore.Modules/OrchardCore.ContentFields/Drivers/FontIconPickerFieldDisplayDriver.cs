using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentFields.Fields
{
    public class FontIconPickerFieldDisplayDriver : ContentFieldDisplayDriver<FontIconPickerField>
    {
        public override IDisplayResult Display(FontIconPickerField field, BuildFieldDisplayContext fieldDisplayContext)
        {
            return Initialize<FontIconPickerFieldViewModel>("FontIconPickerField", model =>
            {
                model.Field = field;
                model.Part = fieldDisplayContext.ContentPart;
                model.PartFieldDefinition = fieldDisplayContext.PartFieldDefinition;
            }).Location("Content").Location("SummaryAdmin", "");
        }

        public override IDisplayResult Edit(FontIconPickerField field, BuildFieldEditorContext context)
        {
            return Initialize<EditFontIconPickerFieldViewModel>(GetEditorShapeType(context), model =>
            {
                model.IconCode = field.IconCode;
                model.IconFormatted = field.IconFormatted;
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(FontIconPickerField field, IUpdateModel updater, UpdateFieldEditorContext context)
        {
            EditFontIconPickerFieldViewModel model = new EditFontIconPickerFieldViewModel();

            await updater.TryUpdateModelAsync(model, Prefix);

            if (string.IsNullOrEmpty(model.IconCode))
            {
                return Edit(field, context);
            }

            field.IconCode = model.IconCode;
            field.IconFormatted = model.IconFormatted;

            return Edit(field, context);
        }
    }
}
