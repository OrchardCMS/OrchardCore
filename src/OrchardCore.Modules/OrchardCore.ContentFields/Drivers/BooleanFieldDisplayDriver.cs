using System.Threading.Tasks;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentFields.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentFields.Drivers
{
    public class BooleanFieldDisplayDriver : ContentFieldDisplayDriver<BooleanField>
    {
        public override IDisplayResult Display(BooleanField field, BuildFieldDisplayContext context)
        {
            return Initialize<DisplayBooleanFieldViewModel>(GetDisplayShapeType(context), model =>
            {
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            })
            .Location("Detail", "Content")
            .Location("Summary", "Content");
        }

        public override IDisplayResult Edit(BooleanField field, BuildFieldEditorContext context)
        {
            return Initialize<EditBooleanFieldViewModel>(GetEditorShapeType(context), model =>
            {
                model.Value = (context.IsNew == false) ?
                    field.Value : context.PartFieldDefinition.GetSettings<BooleanFieldSettings>().DefaultValue;

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
