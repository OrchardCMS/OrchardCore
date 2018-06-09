using System.Threading.Tasks;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentFields.Drivers
{
    public class CoordinateFieldDisplayDriver : ContentFieldDisplayDriver<CoordinateField>
    {
        public override IDisplayResult Display(CoordinateField field, BuildFieldDisplayContext context)
        {
            return Initialize<DisplayCoordinateFieldViewModel>("CoordinateField", model =>
            {
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            })
            .Location("Content")
            .Location("SummaryAdmin", "");
        }

        public override IDisplayResult Edit(CoordinateField field, BuildFieldEditorContext context)
        {
            return Initialize<EditCoordinateFieldViewModel>("CoordinateField_Edit", model =>
            {
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(CoordinateField field, IUpdateModel updater, UpdateFieldEditorContext context)
        {
            await updater.TryUpdateModelAsync(field, Prefix, f => f.Latitude, f => f.Longitude);

            return Edit(field, context); ;
        }
    }
}
