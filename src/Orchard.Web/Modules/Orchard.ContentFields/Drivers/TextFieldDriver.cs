using Orchard.ContentManagement;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentManagement.Drivers;
using System.Threading.Tasks;
using Orchard.DisplayManagement.Views;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.ContentFields.ViewModels;
using Orchard.ContentManagement.MetaData.Models;
using System;

namespace Orchard.ContentFields.Fields
{
    public class TextFieldDriver : ContentFieldDriver<TextField>
    {
    }

    public class TextFieldDisplayDriver : ContentFieldDisplayDriver<TextField>
    {
        public override IDisplayResult Display(TextField field, ContentPart part, ContentPartFieldDefinition partFieldDefinition)
        {
            if (!String.Equals("TextField", partFieldDefinition.FieldDefinition.Name, StringComparison.Ordinal))
            {
                return null;
            }

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
            if (!String.Equals("TextField", partFieldDefinition.FieldDefinition.Name, StringComparison.Ordinal))
            {
                return null;
            }

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
            if (!String.Equals("TextField", partFieldDefinition.FieldDefinition.Name, StringComparison.Ordinal))
            {
                return null;
            }

            await updater.TryUpdateModelAsync(field, Prefix, f => f.Text);

            return Edit(field, part, partFieldDefinition);
        }
    }
}
