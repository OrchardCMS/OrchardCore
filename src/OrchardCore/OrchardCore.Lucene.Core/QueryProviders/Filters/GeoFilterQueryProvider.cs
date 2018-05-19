using System.Linq;
using Lucene.Net.Search;
using Lucene.Net.Spatial.Prefix;
using Lucene.Net.Spatial.Prefix.Tree;
using Lucene.Net.Spatial.Queries;
using Newtonsoft.Json.Linq;
using Spatial4n.Core.Context;

namespace OrchardCore.Lucene.QueryProviders.Filters
{
    public class GeoBoundingBoxFilterProvider : ILuceneBooleanFilterProvider
    {
        public FilteredQuery CreateFilteredQuery(ILuceneQueryService builder, LuceneQueryContext context, string type, JObject queryObj, Query toFilter)
        {
            if (type != "geo_bounding_box")
                return null;

            var ctx = SpatialContext.GEO;

            var maxLevels = 11; //results in sub-meter precision for geohash

            //  This can also be constructed from SpatialPrefixTreeFactory
            SpatialPrefixTree grid = new GeohashPrefixTree(ctx, maxLevels);

            var first = queryObj.Properties().First();
            var geoPropertyName = first.Name;
            var strategy = new RecursivePrefixTreeStrategy(grid, geoPropertyName);

            var boundingBox = (JObject) first.Value;

            var topLeftProperty = boundingBox["top_left"] as JObject;
            var bottomRightProperty =boundingBox["bottom_right"] as JObject;

            if (topLeftProperty == null || bottomRightProperty == null)
                return null;

            var left = topLeftProperty["lon"];
            var top = topLeftProperty["lat"];
            var bottom = bottomRightProperty["lat"];
            var right = bottomRightProperty["lon"];

            var rectangle = ctx.MakeRectangle((double)left, (double)right, (double)bottom, (double)top);

            var args = new SpatialArgs(SpatialOperation.Intersects, rectangle);

            var filter = strategy.MakeFilter(args);

            return new FilteredQuery(toFilter, filter);
        }
    }

}
