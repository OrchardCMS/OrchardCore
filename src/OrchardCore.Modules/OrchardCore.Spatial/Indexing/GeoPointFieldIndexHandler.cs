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
            DocumentIndex.GeoPoint value = null;

            if (field.Longitude != null && field.Latitude != null)
            {
                value = new DocumentIndex.GeoPoint
                {
                    Longitude = (decimal)field.Longitude,
                    Latitude = (decimal)field.Latitude
                };
            }

            foreach (var key in context.Keys)
            {
                context.DocumentIndex.Set(key, value, options);
            }

            // Also index as "Location" to be able to search on multiple Content Types
            context.DocumentIndex.Set("Location", value, DocumentIndexOptions.Store);

            return Task.CompletedTask;
        }
    }
}
