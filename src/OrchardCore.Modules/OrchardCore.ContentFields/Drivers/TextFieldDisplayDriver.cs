using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentFields.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentFields.Drivers
{
    public class TextFieldDisplayDriver : ContentFieldDisplayDriver<TextField>
    {
        private readonly IStringLocalizer S;

        public TextFieldDisplayDriver(IStringLocalizer<TextFieldDisplayDriver> localizer)
        {
            S = localizer;
        }

        public override IDisplayResult Display(TextField field, BuildFieldDisplayContext context)
        {
            return Initialize<DisplayTextFieldViewModel>(GetDisplayShapeType(context), model =>
            {
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            })
            .Location("Detail", "Content")
            .Location("Summary", "Content");
        }

        public override IDisplayResult Edit(TextField field, BuildFieldEditorContext context)
        {
            return Initialize<EditTextFieldViewModel>(GetEditorShapeType(context), model =>
            {
                model.Text = field.Text;
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(TextField field, IUpdateModel updater, UpdateFieldEditorContext context)
        {
            if (await updater.TryUpdateModelAsync(field, Prefix, f => f.Text))
            {
                var settings = context.PartFieldDefinition.GetSettings<TextFieldSettings>();
                if (settings.Required && String.IsNullOrWhiteSpace(field.Text))
                {
                    updater.ModelState.AddModelError(Prefix, S["A value is required for {0}.", context.PartFieldDefinition.DisplayName()]);
                }
            }

            return Edit(field, context);
        }
    }
}
