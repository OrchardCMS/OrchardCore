using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Spatial.Fields;
using OrchardCore.Spatial.Settings;
using OrchardCore.Spatial.ViewModels;

namespace OrchardCore.Spatial.Drivers
{
    public class GeoPointFieldDisplayDriver : ContentFieldDisplayDriver<GeoPointField>
    {
        protected readonly IStringLocalizer S;

        public GeoPointFieldDisplayDriver(IStringLocalizer<GeoPointFieldDisplayDriver> localizer)
        {
            S = localizer;
        }

        public override IDisplayResult Display(GeoPointField field, BuildFieldDisplayContext context)
        {
            return Initialize<DisplayGeoPointFieldViewModel>(GetDisplayShapeType(context), model =>
                {
                    model.Field = field;
                    model.Part = context.ContentPart;
                    model.PartFieldDefinition = context.PartFieldDefinition;
                })
                .Location("Detail", "Content")
                .Location("Summary", "Content");
        }

        public override IDisplayResult Edit(GeoPointField field, BuildFieldEditorContext context)
        {
            return Initialize<EditGeoPointFieldViewModel>(GetEditorShapeType(context), model =>
            {
                model.Latitude = Convert.ToString(field.Latitude, CultureInfo.InvariantCulture);
                model.Longitude = Convert.ToString(field.Longitude, CultureInfo.InvariantCulture);
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(GeoPointField field, IUpdateModel updater, UpdateFieldEditorContext context)
        {
            var viewModel = new EditGeoPointFieldViewModel();

            var modelUpdated = await updater.TryUpdateModelAsync(viewModel, Prefix, f => f.Latitude, f => f.Longitude);

            if (modelUpdated)
            {
                decimal latitude;
                decimal longitude;

                var settings = context.PartFieldDefinition.GetSettings<GeoPointFieldSettings>();

                if (String.IsNullOrWhiteSpace(viewModel.Latitude))
                {
                    if (settings.Required)
                    {
                        updater.ModelState.AddModelError(Prefix, nameof(field.Latitude), S["The {0} field is required.", context.PartFieldDefinition.DisplayName()]);
                    }
                    else
                    {
                        field.Latitude = null;
                    }
                }
                else if (!Decimal.TryParse(viewModel.Latitude, NumberStyles.Any, CultureInfo.InvariantCulture, out latitude))
                {
                    updater.ModelState.AddModelError(Prefix, nameof(viewModel.Latitude), S["{0} is an invalid number.", context.PartFieldDefinition.DisplayName()]);
                }
                else
                {
                    field.Latitude = latitude;
                }

                if (String.IsNullOrWhiteSpace(viewModel.Longitude))
                {
                    if (settings.Required)
                    {
                        updater.ModelState.AddModelError(Prefix, nameof(field.Longitude), S["The {0} field is required.", context.PartFieldDefinition.DisplayName()]);
                    }
                    else
                    {
                        field.Longitude = null;
                    }
                }
                else if (!Decimal.TryParse(viewModel.Longitude, NumberStyles.Any, CultureInfo.InvariantCulture, out longitude))
                {
                    updater.ModelState.AddModelError(Prefix, nameof(viewModel.Longitude), S["{0} is an invalid number.", context.PartFieldDefinition.DisplayName()]);
                }
                else
                {
                    field.Longitude = longitude;
                }
            }

            return Edit(field, context);
        }
    }
}
