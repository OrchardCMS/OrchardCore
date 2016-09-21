using System.Threading.Tasks;
using Orchard.ContentFields.ViewModels;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentManagement.Display.Models;
using Orchard.ContentManagement.Drivers;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;

namespace Orchard.ContentFields.Fields
{
    public class TextFieldDriver : ContentFieldDriver<TextField>
    {
    }

    public class TextFieldDisplayDriver : ContentFieldDisplayDriver<TextField>
    {
        public override IDisplayResult Display(TextField field, BuildFieldDisplayContext context)
        {
            return Shape<DisplayTextFieldViewModel>("TextField", model =>
            {
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            })
            .Location("Content");
        }

        public override IDisplayResult Edit(TextField field, BuildFieldEditorContext context)
        {
            return Shape<EditTextFieldViewModel>("TextField_Edit", model =>
            {
                model.Text = field.Text;
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(TextField field, IUpdateModel updater, UpdateFieldEditorContext context)
        {
            await updater.TryUpdateModelAsync(field, Prefix, f => f.Text);

            return Edit(field, context);
        }
    }
}
