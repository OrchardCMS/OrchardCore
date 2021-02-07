using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentFields.Drivers
{
    public class UserPickerFieldUserNamesDisplayDriver : ContentFieldDisplayDriver<UserPickerField>
    {
        public override IDisplayResult Display(UserPickerField field, BuildFieldDisplayContext context)
        {
            return Initialize<DisplayUserPickerFieldUserNamesViewModel>(GetDisplayShapeType(context), model =>
            {
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            })
            .Location("Detail", "Content")
            .Location("Summary", "Content");
        }
    }
}
