using System.Threading.Tasks;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Spatial.Fields;
using OrchardCore.Spatial.ViewModels;

namespace OrchardCore.Spatial.Drivers
{
    public class GeoPointFieldDisplayDriver : ContentFieldDisplayDriver<GeoPointField>
    {
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
                model.Latitude = field.Latitude;
                model.Longitude = field.Longitude;
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(GeoPointField field, IUpdateModel updater, UpdateFieldEditorContext context)
        {
            await updater.TryUpdateModelAsync(field, Prefix, f => f.Latitude, f => f.Longitude);

            return Edit(field, context);
        }
    }
}
