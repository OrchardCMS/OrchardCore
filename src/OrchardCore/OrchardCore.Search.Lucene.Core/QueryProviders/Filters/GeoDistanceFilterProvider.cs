using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Lucene.Net.Queries.Function;
using Lucene.Net.Search;
using Lucene.Net.Spatial.Prefix;
using Lucene.Net.Spatial.Prefix.Tree;
using Lucene.Net.Spatial.Queries;
using Lucene.Net.Spatial.Util;
using Spatial4n.Context;
using Spatial4n.Distance;
using Spatial4n.Shapes;
using Spatial4n.Util;

namespace OrchardCore.Search.Lucene.QueryProviders.Filters;

public partial class GeoDistanceFilterProvider : ILuceneBooleanFilterProvider
{
    public FilteredQuery CreateFilteredQuery(ILuceneQueryService builder, LuceneQueryContext context, string type, JsonNode filter, Query toFilter)
    {
        if (type != "geo_distance")
        {
            return null;
        }

        if (toFilter is not BooleanQuery booleanQuery)
        {
            return null;
        }

        var queryObj = filter.AsObject();

        if (queryObj.Count != 2)
        {
            return null;
        }

        var ctx = SpatialContext.Geo;

        var maxLevels = 11; // Results in sub-meter precision for geohash.

        // This can also be constructed from SpatialPrefixTreeFactory.
        SpatialPrefixTree grid = new GeohashPrefixTree(ctx, maxLevels);

        JsonNode distanceProperty = null;
        JsonNode geoProperty = null;

        foreach (var jProperty in queryObj)
        {
            if (jProperty.Key.Equals("distance", StringComparison.Ordinal))
            {
                distanceProperty = jProperty.Value;
            }
            else
            {
                geoProperty = jProperty.Value;
            }
        }

        if (distanceProperty == null || geoProperty == null)
        {
            return null;
        }

        var strategy = new RecursivePrefixTreeStrategy(grid, geoProperty.GetPropertyName());

        if (!TryParseDistance(distanceProperty.Value<string>(), out var distanceDegrees))
        {
            return null;
        }

        if (!TryGetPointFromJToken(geoProperty, out var point))
        {
            return null;
        }

        var circle = ctx.MakeCircle(point.X, point.Y, distanceDegrees);

        var args = new SpatialArgs(SpatialOperation.Intersects, circle);

        var spatialQuery = strategy.MakeQuery(args);
        var valueSource = strategy.MakeRecipDistanceValueSource(circle);
        var valueSourceFilter = new ValueSourceFilter(new QueryWrapperFilter(spatialQuery), valueSource, 0, 1);

        booleanQuery.Add(new FunctionQuery(valueSource), Occur.MUST);

        return new FilteredQuery(booleanQuery, valueSourceFilter);
    }

    private static bool TryParseDistance(string distanceValue, out double distanceDegrees)
    {
        distanceDegrees = -1;

        var distanceString = StringDistanceRegex().Match(distanceValue).Value;

        if (string.IsNullOrEmpty(distanceString))
        {
            return false;
        }

        var distanceUnits = distanceValue[distanceString.Length..].ToLower();

        if (!double.TryParse(distanceString, out var distance))
        {
            return false;
        }

        switch (distanceUnits)
        {
            case "mi":
            case "miles":
                distanceDegrees = DistanceUtils.Dist2Degrees(distance, DistanceUtils.EarthMeanRadiusMiles);
                return true;
            case "km":
            case "kilometers":
                distanceDegrees = DistanceUtils.Dist2Degrees(distance, DistanceUtils.EarthMeanRadiusKilometers);
                return true;
            case "ft":
            case "feet":
                distanceDegrees = DistanceUtils.Dist2Degrees(distance / 5280, DistanceUtils.EarthMeanRadiusMiles);
                return true;
            case "yd":
            case "yards":
                distanceDegrees = DistanceUtils.Dist2Degrees(distance / 1760, DistanceUtils.EarthMeanRadiusMiles);
                return true;
            case "in":
            case "inch":
                distanceDegrees = DistanceUtils.Dist2Degrees(distance / 63360, DistanceUtils.EarthMeanRadiusMiles);
                return true;
            case "m":
            case "meters":
                distanceDegrees = DistanceUtils.Dist2Degrees(distance / 1000, DistanceUtils.EarthMeanRadiusKilometers);
                return true;
            case "cm":
            case "centimeters":
                distanceDegrees = DistanceUtils.Dist2Degrees(distance / 100000, DistanceUtils.EarthMeanRadiusKilometers);
                return true;
            case "mm":
            case "millimeters":
                distanceDegrees = DistanceUtils.Dist2Degrees(distance / 1000000, DistanceUtils.EarthMeanRadiusKilometers);
                return true;
            case "nm":
            case "nmi":
            case "nauticalmiles":
                distanceDegrees = DistanceUtils.Dist2Degrees(distance * 1.852, DistanceUtils.EarthMeanRadiusKilometers);
                return true;
        }

        return false;
    }

    private static bool TryGetPointFromJToken(JsonNode geoToken, out IPoint point)
    {
        point = null;

        var ctx = SpatialContext.Geo;

        switch (geoToken.GetValueKind())
        {
            case JsonValueKind.String:
                var geoStringValue = geoToken.ToString();

                var geoStringSplit = geoStringValue.Split(',');

                if (geoStringSplit.Length == 2)
                {
                    if (!double.TryParse(geoStringSplit[0], out var lat))
                    {
                        return false;
                    }

                    if (!double.TryParse(geoStringSplit[1], out var lon))
                    {
                        return false;
                    }

                    point = new Point(lon, lat, ctx);
                    return true;
                }

                point = GeohashUtils.Decode(geoStringValue, ctx);
                return true;
            case JsonValueKind.Object:
                var geoPointValue = geoToken.AsObject();

                if (!geoPointValue.ContainsKey("lon") || !geoPointValue.ContainsKey("lat"))
                {
                    return false;
                }

                point = new Point(geoPointValue["lon"].Value<double>(), geoPointValue["lat"].Value<double>(), ctx);
                return true;
            case JsonValueKind.Array:
                var geoArrayValue = geoToken.AsArray();

                if (geoArrayValue.Count != 2)
                {
                    return false;
                }

                point = new Point(geoArrayValue[0].Value<double>(), geoArrayValue[1].Value<double>(), ctx);

                return true;
            default: throw new ArgumentException("Invalid geo point representation");
        }
    }

    [GeneratedRegex(@"^((\d+(\.\d*)?)|(\.\d+))")]
    private static partial Regex StringDistanceRegex();
}
