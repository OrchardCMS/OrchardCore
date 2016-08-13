using System.Threading.Tasks;
using Orchard.ContentFields.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentManagement.Display.Models;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;

namespace Orchard.ContentFields.Fields
{
    public class BooleanFieldDriver : ContentFieldDriver<BooleanField>
    {
    }

    public class BooleanFieldDisplayDriver : ContentFieldDisplayDriver<BooleanField>
    {
        public override IDisplayResult Display(BooleanField field, BuildFieldDisplayContext context)
        {
            return Shape<DisplayBooleanFieldViewModel>("BooleanField", model =>
            {
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            })
            .Location("Content");
        }

        public override IDisplayResult Edit(BooleanField field, BuildFieldEditorContext context)
        {
            return Shape<EditBooleanFieldViewModel>("BooleanField_Edit", model =>
            {
                model.Value = field.Value;
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(BooleanField field, IUpdateModel updater, UpdateFieldEditorContext context)
        {
            await updater.TryUpdateModelAsync(field, Prefix, f => f.Value);

            return Edit(field, context);
        }
    }
}
