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
                DocumentIndex.GeoPoint value = null;

                if (field.Longitude != null && field.Latitude != null)
                {
                    value = new DocumentIndex.GeoPoint
                    {
                        Longitude = (double)field.Longitude,
                        Latitude = (double)field.Latitude
                    };
                }

                context.DocumentIndex.Set(key, value, options);
            } 

            return Task.CompletedTask;
        }
    }
}
