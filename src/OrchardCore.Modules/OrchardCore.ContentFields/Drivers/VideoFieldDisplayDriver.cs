using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentFields.Fields
{
    public class VideoFieldDisplayDriver : ContentFieldDisplayDriver<VideoField>
    {

        public override IDisplayResult Display(VideoField field, BuildFieldDisplayContext context)
        {
            return Initialize<VideoFieldDisplayViewModel>("VideoField", model =>
           {
               model.Field = field;
               model.Part = context.ContentPart;
               model.PartFieldDefinition = context.PartFieldDefinition;
           }).Location("Content").Location("SummaryAdmin", "");
        }

        public override IDisplayResult Edit(VideoField field, BuildFieldEditorContext context)
        {
            return Initialize<EditVideoFieldViewModel>("VideoField_Edit", model =>
            {
                model.Address = field.Address;
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(VideoField field, IUpdateModel updater, UpdateFieldEditorContext context)
        {
            await updater.TryUpdateModelAsync(field, Prefix, f => f.Address);
            return Edit(field, context);
        }
    }
}
