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
    public class YoutubeVideoFieldDisplayDriver : ContentFieldDisplayDriver<YoutubeVideoField>
    {

        public override IDisplayResult Display(YoutubeVideoField field, BuildFieldDisplayContext context)
        {
            return Initialize<YoutubeVideoFieldDisplayViewModel>("YoutubeVideoField", model =>
           {
               model.Field = field;
               model.Part = context.ContentPart;
               model.PartFieldDefinition = context.PartFieldDefinition;
           }).Location("Content").Location("SummaryAdmin", "");
        }

        public override IDisplayResult Edit(YoutubeVideoField field, BuildFieldEditorContext context)
        {
            return Initialize<EditYoutubeVideoFieldViewModel >("YoutubeVideoField_Edit", model =>
            {
                model.Address = field.Address;
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(YoutubeVideoField field, IUpdateModel updater, UpdateFieldEditorContext context)
        {
            await updater.TryUpdateModelAsync(field, Prefix, f => f.Address);
            return Edit(field, context);
        }
    }
}
