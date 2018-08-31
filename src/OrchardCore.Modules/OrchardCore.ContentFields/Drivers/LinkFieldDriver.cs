using System;
using System.Threading.Tasks;
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
    public class LinkFieldDisplayDriver : ContentFieldDisplayDriver<LinkField>
    {
        public LinkFieldDisplayDriver(IStringLocalizer<LinkFieldDisplayDriver> localizer)
        {
            T = localizer;
        }

        public IStringLocalizer T { get; set; }

        public override IDisplayResult Display(LinkField field, BuildFieldDisplayContext context)
        {
            return Initialize<DisplayLinkFieldViewModel>("LinkField", model =>
            {
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            })
            .Location("Content")
            .Location("SummaryAdmin", "");
        }

        public override IDisplayResult Edit(LinkField field, BuildFieldEditorContext context)
        {
            return Initialize<EditLinkFieldViewModel>(GetEditorShapeType(context), model =>
            {
                var settings = context.PartFieldDefinition.Settings.ToObject<LinkFieldSettings>();
                model.Url = context.IsNew ? settings.DefaultUrl : field.Url;
                model.Text = context.IsNew ? settings.DefaultText : field.Text;
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(LinkField field, IUpdateModel updater, UpdateFieldEditorContext context)
        {
            bool modelUpdated = await updater.TryUpdateModelAsync(field, Prefix, f => f.Url, f => f.Text);

            if (modelUpdated)
            {                
                var settings = context.PartFieldDefinition.Settings.ToObject<LinkFieldSettings>();

                if (settings.Required && String.IsNullOrWhiteSpace(field.Url))
                {
                    updater.ModelState.AddModelError(Prefix, T["The url is required for {0}.", context.PartFieldDefinition.DisplayName()]);
                }
                else if (!string.IsNullOrWhiteSpace(field.Url) && !Uri.IsWellFormedUriString(field.Url, UriKind.RelativeOrAbsolute))
                {
                    updater.ModelState.AddModelError(Prefix, T["{0} is an invalid url.", field.Url]);
                }
                else if (settings.LinkTextMode == LinkTextMode.Required && string.IsNullOrWhiteSpace(field.Text))
                {
                    updater.ModelState.AddModelError(Prefix, T["The link text is required for {0}.", context.PartFieldDefinition.DisplayName()]);
                }
                else if (settings.LinkTextMode == LinkTextMode.Static && string.IsNullOrWhiteSpace(settings.DefaultText))
                {
                    updater.ModelState.AddModelError(Prefix, T["The text default value is required for {0}.", context.PartFieldDefinition.DisplayName()]);
                }
            }

            return Edit(field, context);
        }
    }
}
