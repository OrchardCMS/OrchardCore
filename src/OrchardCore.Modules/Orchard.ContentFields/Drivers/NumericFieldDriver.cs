using System;
using System.Globalization;
using System.Threading.Tasks;
using Orchard.ContentFields.Settings;
using Orchard.ContentFields.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentManagement.Display.Models;
using Orchard.ContentManagement.Metadata.Models;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;
using Microsoft.Extensions.Localization;

namespace Orchard.ContentFields.Fields
{
    public class NumericFieldDisplayDriver : ContentFieldDisplayDriver<NumericField>
    {
        private readonly CultureInfo _cultureInfo;

        public NumericFieldDisplayDriver(IStringLocalizer<LinkFieldDisplayDriver> localizer)
        {
            T = localizer;

            _cultureInfo = CultureInfo.InvariantCulture; // Todo: Get CurrentCulture from site settings
        }

        public IStringLocalizer T { get; set; }

        public override IDisplayResult Display(NumericField field, BuildFieldDisplayContext context)
        {
            return Shape<DisplayNumericFieldViewModel>("NumericField", model =>
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
            return Shape<EditNumericFieldViewModel>("NumericField_Edit", model =>
            {
                var settings = context.PartFieldDefinition.Settings.ToObject<NumericFieldSettings>();
                model.Value = (field.IsNew() && field.Value == null) ? settings.DefaultValue : Convert.ToString(field.Value, _cultureInfo);

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
                else if (!decimal.TryParse(viewModel.Value, NumberStyles.Any, _cultureInfo, out value))
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
