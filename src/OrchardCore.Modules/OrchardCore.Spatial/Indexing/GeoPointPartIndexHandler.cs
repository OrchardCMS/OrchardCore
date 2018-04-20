using System;
using System.Collections.Generic;
using System.Text;
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
                Longitude = part.Longitude,
                Latitude = part.Latitude
            }, DocumentIndex.Types.GeoPoint, options));

            return Task.CompletedTask;
        }
    }
}
