using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
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
            return Initialize<EditYoutubeVideoFieldViewModel>("YoutubeVideoField_Edit", model =>
           {
               model.RawAddress = field.RawAddress;
               model.EmbeddedAddress = field.EmbeddedAddress;
               model.Field = field;
               model.Part = context.ContentPart;
               model.PartFieldDefinition = context.PartFieldDefinition;
           });
        }

        public override async Task<IDisplayResult> UpdateAsync(YoutubeVideoField field, IUpdateModel updater, UpdateFieldEditorContext context)
        {
            EditYoutubeVideoFieldViewModel model = new EditYoutubeVideoFieldViewModel();

            await updater.TryUpdateModelAsync(model, Prefix);

            var uri = new Uri(model.RawAddress);

            // if it is a url with QueryString
            if (!string.IsNullOrWhiteSpace(uri.Query))
            {
                var query = QueryHelpers.ParseQuery(uri.Query);
                if (query.ContainsKey("v"))
                {
                    model.EmbeddedAddress = $"{uri.GetLeftPart(UriPartial.Authority)}/embed/{query["v"]}";
                }
                else
                {
                    updater.ModelState.AddModelError(Prefix + "." + nameof(model.RawAddress), "The url format is not correct");
                }
            }
            else
            {
                string path = uri.AbsolutePath.Split('?')[0];
                model.EmbeddedAddress = $"{uri.GetLeftPart(UriPartial.Authority)}/embed/{path}";
            }

            field.RawAddress = model.RawAddress;
            field.EmbeddedAddress = model.EmbeddedAddress;

            return Edit(field, context);
        }
    }
}
