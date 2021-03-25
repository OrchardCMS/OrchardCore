using System.Threading.Tasks;
using OrchardCore.Indexing;
using OrchardCore.Spatial.Fields;

namespace OrchardCore.Spatial.Indexing
{
    public class GeoPointFieldIndexHandler : ContentFieldIndexHandler<GeoPointField>
    {
        public override Task BuildIndexAsync(GeoPointField field, BuildFieldIndexContext context)
        {
            var options = context.Settings.ToOptions();
            foreach (var key in context.Keys)
            {
                context.DocumentIndex.Set(key, new DocumentIndex.GeoPoint
                {
                    Longitude = field.Longitude,
                    Latitude = field.Latitude
                }, options);
            }

            return Task.CompletedTask;
        }
    }
}
