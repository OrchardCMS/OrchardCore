using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentFields.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentFields.Fields
{
    public class YoutubeFieldDisplayDriver : ContentFieldDisplayDriver<YoutubeField>
    {
        public YoutubeFieldDisplayDriver(IStringLocalizer<YoutubeFieldDisplayDriver> localizer)
        {
            T = localizer;
        }

        public IStringLocalizer T { get; set; }

        public override IDisplayResult Display(YoutubeField field, BuildFieldDisplayContext context)
        {
            return Initialize<YoutubeFieldDisplayViewModel>("YoutubeField", model =>
           {
               model.Field = field;
               model.Part = context.ContentPart;
               model.PartFieldDefinition = context.PartFieldDefinition;
           }).Location("Content").Location("SummaryAdmin", "");
        }

        public override IDisplayResult Edit(YoutubeField field, BuildFieldEditorContext context)
        {
            return Initialize<EditYoutubeFieldViewModel>(GetEditorShapeType(context), model =>
           {
               model.RawAddress = field.RawAddress;
               model.EmbeddedAddress = field.EmbeddedAddress;
               model.Field = field;
               model.Part = context.ContentPart;
               model.PartFieldDefinition = context.PartFieldDefinition;
           });
        }

        public override async Task<IDisplayResult> UpdateAsync(YoutubeField field, IUpdateModel updater, UpdateFieldEditorContext context)
        {
            EditYoutubeFieldViewModel model = new EditYoutubeFieldViewModel();

            if (await updater.TryUpdateModelAsync(model, Prefix))
            {
                var settings = context.PartFieldDefinition.Settings.ToObject<YoutubeFieldSettings>();
                if (settings.Required && String.IsNullOrWhiteSpace(model.RawAddress))
                {
                    updater.ModelState.AddModelError(Prefix, T["A value is required for '{0}'.", context.PartFieldDefinition.DisplayName()]);
                }
                else
                {
                    var uri = new Uri(model.RawAddress);

                    // if it is a url with QueryString
                    if (!String.IsNullOrWhiteSpace(uri.Query))
                    {
                        var query = QueryHelpers.ParseQuery(uri.Query);
                        if (query.ContainsKey("v"))
                        {
                            model.EmbeddedAddress = $"{uri.GetLeftPart(UriPartial.Authority)}/embed/{query["v"]}";
                        }
                        else
                        {
                            updater.ModelState.AddModelError(Prefix + "." + nameof(model.RawAddress), T["The format of the url is invalid"]);
                        }
                    }
                    else
                    {
                        var path = uri.AbsolutePath.Split('?')[0];
                        model.EmbeddedAddress = $"{uri.GetLeftPart(UriPartial.Authority)}/embed/{path}";
                    }

                    field.RawAddress = model.RawAddress;
                    field.EmbeddedAddress = model.EmbeddedAddress;
                }
            }

            return Edit(field, context);
        }
    }
}
