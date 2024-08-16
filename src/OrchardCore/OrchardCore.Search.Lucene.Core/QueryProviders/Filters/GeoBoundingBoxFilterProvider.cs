using System.Text.Json.Nodes;
using Lucene.Net.Queries.Function;
using Lucene.Net.Search;
using Lucene.Net.Spatial.Prefix;
using Lucene.Net.Spatial.Prefix.Tree;
using Lucene.Net.Spatial.Queries;
using Lucene.Net.Spatial.Util;
using Spatial4n.Context;

namespace OrchardCore.Search.Lucene.QueryProviders.Filters;

public class GeoBoundingBoxFilterProvider : ILuceneBooleanFilterProvider
{
    public FilteredQuery CreateFilteredQuery(ILuceneQueryService builder, LuceneQueryContext context, string type, JsonNode filter, Query toFilter)
    {
        if (type != "geo_bounding_box")
        {
            return null;
        }

        if (toFilter is not BooleanQuery booleanQuery)
        {
            return null;
        }

        var queryObj = filter.AsObject();
        var first = queryObj.First();

        var ctx = SpatialContext.Geo;

        var maxLevels = 11; // results in sub-meter precision for geohash

        // This can also be constructed from SpatialPrefixTreeFactory
        SpatialPrefixTree grid = new GeohashPrefixTree(ctx, maxLevels);

        var geoPropertyName = first.Key;
        var strategy = new RecursivePrefixTreeStrategy(grid, geoPropertyName);

        var boundingBox = first.Value.AsObject();

        var topLeftProperty = boundingBox["top_left"].AsObject();
        var bottomRightProperty = boundingBox["bottom_right"].AsObject();

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
