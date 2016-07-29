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
    public class BooleanFieldDriver : ContentFieldDriver<BooleanField>
    {
    }

    public class BooleanFieldDisplayDriver : ContentFieldDisplayDriver<BooleanField>
    {
        public override IDisplayResult Display(BooleanField field, ContentPart part, ContentPartFieldDefinition partFieldDefinition)
        {
            if (!String.Equals("BooleanField", partFieldDefinition.FieldDefinition.Name, StringComparison.Ordinal))
            {
                return null;
            }

            return Shape<DisplayBooleanFieldViewModel>("BooleanField", model =>
            {
                model.Field = field;
                model.Part = part;
                model.PartFieldDefinition = partFieldDefinition;
            })
            .Location("Content");
        }

        public override IDisplayResult Edit(BooleanField field, ContentPart part, ContentPartFieldDefinition partFieldDefinition)
        {
            if (!String.Equals("BooleanField", partFieldDefinition.FieldDefinition.Name, StringComparison.Ordinal))
            {
                return null;
            }

            return Shape<EditBooleanFieldViewModel>("BooleanField_Edit", model =>
            {
                model.Value = field.Value;
                model.Field = field;
                model.Part = part;
                model.PartFieldDefinition = partFieldDefinition;
            })
            .Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(BooleanField field, ContentPart part, ContentPartFieldDefinition partFieldDefinition, IUpdateModel updater)
        {
            if (!String.Equals("BooleanField", partFieldDefinition.FieldDefinition.Name, StringComparison.Ordinal))
            {
                return null;
            }

            await updater.TryUpdateModelAsync(field, Prefix, f => f.Value);

            return Edit(field, part, partFieldDefinition);
        }
    }
}
