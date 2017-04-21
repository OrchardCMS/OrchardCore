using System.Threading.Tasks;
using Orchard.ContentFields.ViewModels;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentManagement.Display.Models;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;

namespace Orchard.ContentFields.Fields
{
    public class EnumerationFieldDisplayDriver : ContentFieldDisplayDriver<EnumerationField>
    {
        public override IDisplayResult Display(EnumerationField field, BuildFieldDisplayContext context)
        {
            return Shape<DisplayEnumerationFieldViewModel>("EnumerationField", model =>
            {
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            })
            .Location("Content")
            .Location("SummaryAdmin", "");
            ;
        }

        public override IDisplayResult Edit(EnumerationField field, BuildFieldEditorContext context)
        {
            return Shape<EditEnumerationFieldViewModel>("EnumerationField_Edit", model =>
            {
                model.Value = field.Value;
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(EnumerationField field, IUpdateModel updater, UpdateFieldEditorContext context)
        {
            await updater.TryUpdateModelAsync(field, Prefix, f => f.Value);

            return Edit(field, context);
        }
    }
}
