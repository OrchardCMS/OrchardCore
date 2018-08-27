using System;
using System.Threading.Tasks;
using OrchardCore.ContentFields.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentFields.Fields
{
    public class ContentPickerFieldDisplayDriver : ContentFieldDisplayDriver<ContentPickerField>
    {
        public override IDisplayResult Display(ContentPickerField field, BuildFieldDisplayContext context)
        {
            return Initialize<DisplayContentPickerFieldViewModel>("ContentPickerField", model =>
            {
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            })
            .Location("Content")
            .Location("SummaryAdmin", "");
        }

        public override IDisplayResult Edit(ContentPickerField field, BuildFieldEditorContext context)
        {
            return Initialize<EditContentPickerFieldViewModel>(GetEditorShapeType(context), model =>
            {
                model.ContentItemIds = string.Join(",", field.ContentItemIds);

                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentPickerField field, IUpdateModel updater, UpdateFieldEditorContext context)
        {
            var viewModel = new EditContentPickerFieldViewModel();

            await updater.TryUpdateModelAsync(viewModel, Prefix, f => f.ContentItemIds);

            field.ContentItemIds = viewModel.ContentItemIds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            return Edit(field, context);
        }
    }
}
