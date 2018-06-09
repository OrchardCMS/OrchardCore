using System.Threading.Tasks;
using OrchardCore.Indexing;

namespace OrchardCore.Spatial.Indexing
{
    public class GeoPointPartIndexHandler : ContentPartIndexHandler<GeoPointPart>
    {
        public override Task BuildIndexAsync(GeoPointPart part, BuildPartIndexContext context)
        {
            var options = context.Settings.ToOptions()
                          | DocumentIndexOptions.Sanitize
                          | DocumentIndexOptions.Analyze
                ;

            context.DocumentIndex.Entries.Add(context.Key, new DocumentIndex.DocumentIndexEntry(new DocumentIndex.Point()
            {
                X = part.Longitude,
                Y = part.Latitude
            }, DocumentIndex.Types.GeoPoint, options));

            return Task.CompletedTask;
        }
    }
}
