using System.Threading.Tasks;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentFields.Drivers
{
    public class TimeFieldDisplayDriver : ContentFieldDisplayDriver<TimeField>
    {
        public override IDisplayResult Display(TimeField field, BuildFieldDisplayContext context)
        {
            return Initialize<DisplayTimeFieldViewModel>(GetDisplayShapeType(context), model =>
            {
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            })
            .Location("Detail", "Content")
            .Location("Summary", "Content");
        }

        public override IDisplayResult Edit(TimeField field, BuildFieldEditorContext context)
        {
            return Initialize<EditTimeFieldViewModel>(GetEditorShapeType(context), model =>
            {
                model.Value = field.Value;
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(TimeField field, IUpdateModel updater, UpdateFieldEditorContext context)
        {
            await updater.TryUpdateModelAsync(field, Prefix, f => f.Value);

            return Edit(field, context);
        }
    }
}
