using System;
using System.Threading.Tasks;
using OrchardCore.ContentFields.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentFields.Fields
{
    public class DateFieldDisplayDriver : ContentFieldDisplayDriver<DateField>
    {
        public override IDisplayResult Display(DateField field, BuildFieldDisplayContext context)
        {
            return Initialize<DisplayDateFieldViewModel>("DateField", model =>
            {
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            })
            .Location("Content")
            .Location("SummaryAdmin", "");
            ;
        }

        public override IDisplayResult Edit(DateField field, BuildFieldEditorContext context)
        {
            return Initialize<EditDateFieldViewModel>(GetEditorShapeType(context), model =>
            {
                model.Value = field.Value;
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(DateField field, IUpdateModel updater, UpdateFieldEditorContext context)
        {
            await updater.TryUpdateModelAsync(field, Prefix, f => f.Value);

            return Edit(field, context);
        }
    }
}
