using System.Threading.Tasks;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Spatial.Model;
using OrchardCore.Spatial.ViewModels;

namespace OrchardCore.Spatial.Drivers
{
    public class GeoPointPartDisplayDriver : ContentPartDisplayDriver <GeoPointPart>
    {
        public override IDisplayResult Display(GeoPointPart geoPointPart)
        {
            return Initialize<GeoPointPartViewModel>("GeoPointPart", m => BuildViewModel(m, geoPointPart))
                .Location("Content:10");
        }

        public override IDisplayResult Edit(GeoPointPart geoPointPart)
        {
            return Initialize<GeoPointPartViewModel>("GeoPointPart_Edit", m => BuildViewModel(m, geoPointPart))
                .Location("Content:7.5");
        }

        public override async Task<IDisplayResult> UpdateAsync(GeoPointPart model, IUpdateModel updater)
        {
            await updater.TryUpdateModelAsync(model, Prefix, t => t.Latitude, t => t.Longitude);

            return Edit(model);
        }

        private void BuildViewModel(GeoPointPartViewModel model, GeoPointPart geoPointPart)
        {
            model.Latitude = geoPointPart.Latitude;
            model.Longitude = geoPointPart.Longitude;
            model.GeoPointPart = geoPointPart;
            model.ContentItem = geoPointPart.ContentItem;
        }
    }
}
