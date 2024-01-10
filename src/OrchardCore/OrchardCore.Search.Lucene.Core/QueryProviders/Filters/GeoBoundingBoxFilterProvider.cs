using System.Linq;
using Lucene.Net.Queries.Function;
using Lucene.Net.Search;
using Lucene.Net.Spatial.Prefix;
using Lucene.Net.Spatial.Prefix.Tree;
using Lucene.Net.Spatial.Queries;
using Lucene.Net.Spatial.Util;
using Newtonsoft.Json.Linq;
using Spatial4n.Context;

namespace OrchardCore.Search.Lucene.QueryProviders.Filters
{
    public class GeoBoundingBoxFilterProvider : ILuceneBooleanFilterProvider
    {
        public FilteredQuery CreateFilteredQuery(ILuceneQueryService builder, LuceneQueryContext context, string type, JToken filter, Query toFilter)
        {
            if (type != "geo_bounding_box")
            {
                return null;
            }

            if (toFilter is not BooleanQuery booleanQuery)
            {
                return null;
            }

            var queryObj = filter as JObject;
            var first = queryObj.Properties().First();

            var ctx = SpatialContext.Geo;

            var maxLevels = 11; //results in sub-meter precision for geohash

            //  This can also be constructed from SpatialPrefixTreeFactory
            SpatialPrefixTree grid = new GeohashPrefixTree(ctx, maxLevels);

            var geoPropertyName = first.Name;
            var strategy = new RecursivePrefixTreeStrategy(grid, geoPropertyName);

            var boundingBox = (JObject)first.Value;

            var topLeftProperty = boundingBox["top_left"] as JObject;
            var bottomRightProperty = boundingBox["bottom_right"] as JObject;

            if (topLeftProperty == null || bottomRightProperty == null)
            {
                return null;
            }

            var left = topLeftProperty["lon"];
            var top = topLeftProperty["lat"];
            var bottom = bottomRightProperty["lat"];
            var right = bottomRightProperty["lon"];

            var rectangle = ctx.MakeRectangle((double)left, (double)right, (double)bottom, (double)top);

            var args = new SpatialArgs(SpatialOperation.Intersects, rectangle);

            var spatialQuery = strategy.MakeQuery(args);
            var valueSource = strategy.MakeRecipDistanceValueSource(rectangle);
            var valueSourceFilter = new ValueSourceFilter(new QueryWrapperFilter(spatialQuery), valueSource, 0, 1);

            booleanQuery.Add(new FunctionQuery(valueSource), Occur.MUST);

            return new FilteredQuery(booleanQuery, valueSourceFilter);
        }
    }
}
