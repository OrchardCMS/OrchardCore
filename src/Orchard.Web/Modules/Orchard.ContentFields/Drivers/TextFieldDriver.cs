using System.Threading.Tasks;
using Orchard.ContentFields.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;

namespace Orchard.ContentFields.Fields
{
    public class TextFieldDriver : ContentFieldDriver<TextField>
    {
    }

    public class TextFieldDisplayDriver : ContentFieldDisplayDriver<TextField>
    {
        public override IDisplayResult Display(TextField field, ContentPart part, ContentPartFieldDefinition partFieldDefinition)
        {
            return Shape<DisplayTextFieldViewModel>("TextField", model =>
            {
                model.Field = field;
                model.Part = part;
                model.PartFieldDefinition = partFieldDefinition;
            })
            .Location("Content");
        }

        public override IDisplayResult Edit(TextField field, ContentPart part, ContentPartFieldDefinition partFieldDefinition)
        {
            return Shape<EditTextFieldViewModel>("TextField_Edit", model =>
            {
                model.Text = field.Text;
                model.Field = field;
                model.Part = part;
                model.PartFieldDefinition = partFieldDefinition;
            })
            .Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(TextField field, ContentPart part, ContentPartFieldDefinition partFieldDefinition, IUpdateModel updater)
        {
            await updater.TryUpdateModelAsync(field, Prefix, f => f.Text);

            return Edit(field, part, partFieldDefinition);
        }
    }
}
