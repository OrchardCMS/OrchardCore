using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentFields.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;

namespace OrchardCore.ContentFields.Drivers
{
    public class YoutubeFieldDisplayDriver : ContentFieldDisplayDriver<YoutubeField>
    {
        protected readonly IStringLocalizer S;

        public YoutubeFieldDisplayDriver(IStringLocalizer<YoutubeFieldDisplayDriver> localizer)
        {
            S = localizer;
        }

        public override IDisplayResult Display(YoutubeField field, BuildFieldDisplayContext context)
        {
            return Initialize<YoutubeFieldDisplayViewModel>(GetDisplayShapeType(context), model =>
            {
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            })
            .Location("Detail", "Content")
            .Location("Summary", "Content");
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
            var model = new EditYoutubeFieldViewModel();

            if (await updater.TryUpdateModelAsync(model, Prefix))
            {
                var settings = context.PartFieldDefinition.GetSettings<YoutubeFieldSettings>();
                if (settings.Required && String.IsNullOrWhiteSpace(model.RawAddress))
                {
                    updater.ModelState.AddModelError(Prefix, nameof(model.RawAddress), S["A value is required for '{0}'.", context.PartFieldDefinition.DisplayName()]);
                }
                else
                {
                    if (model.RawAddress != null)
                    {
                        var uri = new Uri(model.RawAddress);

                        // If it is a url with QueryString.
                        if (!String.IsNullOrWhiteSpace(uri.Query))
                        {
                            var query = QueryHelpers.ParseQuery(uri.Query);
                            if (query.TryGetValue("v", out var values))
                            {
                                model.EmbeddedAddress = $"{uri.GetLeftPart(UriPartial.Authority)}/embed/{values}";
                            }
                            else
                            {
                                updater.ModelState.AddModelError(Prefix, nameof(model.RawAddress), S["The format of the url is invalid"]);
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
                    else
                    {
                        field.RawAddress = null;
                        field.EmbeddedAddress = null;
                    }
                }
            }

            return Edit(field, context);
        }
    }
}
