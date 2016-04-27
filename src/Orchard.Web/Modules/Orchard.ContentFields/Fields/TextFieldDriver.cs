using Orchard.ContentManagement;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentManagement.Drivers;
using System.Threading.Tasks;
using Orchard.DisplayManagement.Views;
using Orchard.DisplayManagement.ModelBinding;

namespace Orchard.ContentFields.Fields
{
    public class TextField : ContentField
    {
        public string Text { get; set; }
    }

    public class TextFieldDriver : ContentFieldDriver<TextField>
    {
    }

    public class TextFieldDisplayDriver : ContentFieldDisplayDriver<TextField>
    {
        public override IDisplayResult Display(TextField field, ContentPart part)
        {
            return Shape("TextField", field).Location("Content");
        }

        public override IDisplayResult Edit(TextField field, ContentPart part)
        {
            return Shape("TextField_Edit", field).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(TextField field, ContentPart part, IUpdateModel updater)
        {
            await updater.TryUpdateModelAsync(field, typeof(TextField), "");

            return Edit(field, part);
        }
    }
}
