using System;
using System.Globalization;
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
    public class NumericFieldDisplayDriver : ContentFieldDisplayDriver<NumericField>
    {
        public NumericFieldDisplayDriver(IStringLocalizer<NumericFieldDisplayDriver> localizer)
        {
            T = localizer;
        }

        public IStringLocalizer T { get; set; }

        public override IDisplayResult Display(NumericField field, BuildFieldDisplayContext context)
        {
            return Initialize<DisplayNumericFieldViewModel>("NumericField", model =>
            {
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            })
            .Location("Content")
            .Location("SummaryAdmin", "");
        }

        public override IDisplayResult Edit(NumericField field, BuildFieldEditorContext context)
        {
            return Initialize<EditNumericFieldViewModel>(GetEditorShapeType(context), model =>
            {
                var settings = context.PartFieldDefinition.Settings.ToObject<NumericFieldSettings>();
                model.Value = context.IsNew ? settings.DefaultValue : Convert.ToString(field.Value, CultureInfo.CurrentUICulture);

                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(NumericField field, IUpdateModel updater, UpdateFieldEditorContext context)
        {
            var viewModel = new EditNumericFieldViewModel();

            bool modelUpdated = await updater.TryUpdateModelAsync(viewModel, Prefix, f => f.Value);
            
            if (modelUpdated)
            {
                decimal value;

                var settings = context.PartFieldDefinition.Settings.ToObject<NumericFieldSettings>();

                field.Value = null;

                if (string.IsNullOrWhiteSpace(viewModel.Value))
                {
                    if (settings.Required)
                    {
                        updater.ModelState.AddModelError(Prefix, T["The {0} field is required.", context.PartFieldDefinition.DisplayName()]);
                    }
                }
                else if (!decimal.TryParse(viewModel.Value, NumberStyles.Any, CultureInfo.CurrentUICulture, out value))
                {
                    updater.ModelState.AddModelError(Prefix, T["{0} is an invalid number.", context.PartFieldDefinition.DisplayName()]);
                }
                else
                {
                    field.Value = value;

                    if (settings.Minimum.HasValue && value < settings.Minimum.Value)
                    {
                        updater.ModelState.AddModelError(Prefix, T["The value must be greater than {0}.", settings.Minimum.Value]);
                    }

                    if (settings.Maximum.HasValue && value > settings.Maximum.Value)
                    {
                        updater.ModelState.AddModelError(Prefix, T["The value must be less than {0}.", settings.Maximum.Value]);
                    }

                    // checking the number of decimals
                    if (Math.Round(value, settings.Scale) != value)
                    {
                        if (settings.Scale == 0)
                        {
                            updater.ModelState.AddModelError(Prefix, T["The {0} field must be an integer.", context.PartFieldDefinition.DisplayName()]);
                        }
                        else
                        {
                            updater.ModelState.AddModelError(Prefix, T["Invalid number of digits for {0}, max allowed: {1}.", context.PartFieldDefinition.DisplayName(), settings.Scale]);
                        }
                    }
                }
            }

            return Edit(field, context);
        }
    }
}
