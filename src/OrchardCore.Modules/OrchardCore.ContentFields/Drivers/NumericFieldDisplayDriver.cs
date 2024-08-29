using System.Globalization;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentFields.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;

namespace OrchardCore.ContentFields.Drivers;

public sealed class NumericFieldDisplayDriver : ContentFieldDisplayDriver<NumericField>
{
    internal readonly IStringLocalizer S;

    public NumericFieldDisplayDriver(IStringLocalizer<NumericFieldDisplayDriver> localizer)
    {
        S = localizer;
    }

    public override IDisplayResult Display(NumericField field, BuildFieldDisplayContext context)
    {
        return Initialize<DisplayNumericFieldViewModel>(GetDisplayShapeType(context), model =>
        {
            model.Field = field;
            model.Part = context.ContentPart;
            model.PartFieldDefinition = context.PartFieldDefinition;
        })
        .Location("Detail", "Content")
        .Location("Summary", "Content");
    }

    public override IDisplayResult Edit(NumericField field, BuildFieldEditorContext context)
    {
        return Initialize<EditNumericFieldViewModel>(GetEditorShapeType(context), model =>
        {
            var settings = context.PartFieldDefinition.GetSettings<NumericFieldSettings>();

            // The default value of a field is intended for the editor when a new content item
            // is created (not for APIs). Since we may want to render the editor of a content
            // item that was created by code, we only set the default value in the <input>
            // of the field if it doesn't already have a value.

            if (field.Value.HasValue)
            {
                model.Value = Convert.ToString(field.Value, CultureInfo.CurrentUICulture);
            }
            else if (context.IsNew)
            {
                // The content item is new and the field is not initialized, we can 
                // use the default value from the settings in the editor.
                model.Value = settings.DefaultValue;
            }

            model.Field = field;
            model.Part = context.ContentPart;
            model.PartFieldDefinition = context.PartFieldDefinition;
        });
    }

    public override async Task<IDisplayResult> UpdateAsync(NumericField field, UpdateFieldEditorContext context)
    {
        var viewModel = new EditNumericFieldViewModel();

        await context.Updater.TryUpdateModelAsync(viewModel, Prefix, f => f.Value);
        var settings = context.PartFieldDefinition.GetSettings<NumericFieldSettings>();

        field.Value = null;

        if (string.IsNullOrWhiteSpace(viewModel.Value))
        {
            if (settings.Required)
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(field.Value), S["A value is required for {0}.", context.PartFieldDefinition.DisplayName()]);
            }
        }
        else if (!decimal.TryParse(viewModel.Value, NumberStyles.Any, CultureInfo.CurrentUICulture, out var value))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(field.Value), S["{0} is an invalid number.", context.PartFieldDefinition.DisplayName()]);
        }
        else
        {
            field.Value = value;

            if (settings.Minimum.HasValue && value < settings.Minimum.Value)
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(field.Value), S["The value must be greater than {0}.", settings.Minimum.Value]);
            }

            if (settings.Maximum.HasValue && value > settings.Maximum.Value)
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(field.Value), S["The value must be less than {0}.", settings.Maximum.Value]);
            }

            // Check the number of decimals.
            if (Math.Round(value, settings.Scale) != value)
            {
                if (settings.Scale == 0)
                {
                    context.Updater.ModelState.AddModelError(Prefix, nameof(field.Value), S["The {0} field must be an integer.", context.PartFieldDefinition.DisplayName()]);
                }
                else
                {
                    context.Updater.ModelState.AddModelError(Prefix, nameof(field.Value), S["Invalid number of digits for {0}, max allowed: {1}.", context.PartFieldDefinition.DisplayName(), settings.Scale]);
                }
            }
        }

        return Edit(field, context);
    }
}
