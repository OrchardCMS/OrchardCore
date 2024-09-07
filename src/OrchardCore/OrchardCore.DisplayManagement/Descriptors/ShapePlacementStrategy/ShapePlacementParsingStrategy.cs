using System.Text.Json;
using Microsoft.Extensions.Hosting;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;

namespace OrchardCore.DisplayManagement.Descriptors.ShapePlacementStrategy;

/// <summary>
/// This component discovers and announces the shape alterations implied by the contents of the Placement.json files.
/// </summary>
public class ShapePlacementParsingStrategy : ShapeTableProvider, IShapeTableHarvester
{
    private readonly IHostEnvironment _hostingEnvironment;
    private readonly IShellFeaturesManager _shellFeaturesManager;
    private readonly IEnumerable<IPlacementNodeFilterProvider> _placementParseMatchProviders;

    public ShapePlacementParsingStrategy(
        IHostEnvironment hostingEnvironment,
        IShellFeaturesManager shellFeaturesManager,
        IEnumerable<IPlacementNodeFilterProvider> placementParseMatchProviders)
    {
        _hostingEnvironment = hostingEnvironment;
        _shellFeaturesManager = shellFeaturesManager;
        _placementParseMatchProviders = placementParseMatchProviders;
    }

    public override async ValueTask DiscoverAsync(ShapeTableBuilder builder)
    {
        var enabledFeatures = (await _shellFeaturesManager.GetEnabledFeaturesAsync())
            .Where(Feature => !builder.ExcludedFeatureIds.Contains(Feature.Id));

        foreach (var featureDescriptor in enabledFeatures)
        {
            await ProcessFeatureDescriptorAsync(builder, featureDescriptor);
        }
    }

    private Task ProcessFeatureDescriptorAsync(ShapeTableBuilder builder, IFeatureInfo featureDescriptor)
    {
        // TODO : (ngm) Replace with configuration Provider and read from that.
        // Dont use JSON Deserializer directly.
        var virtualFileInfo = _hostingEnvironment
            .GetExtensionFileInfo(featureDescriptor.Extension, "placement.json");

        if (virtualFileInfo.Exists)
        {
            using var stream = virtualFileInfo.CreateReadStream();

            var placementFile = JsonSerializer.Deserialize<PlacementFile>(stream, JOptions.Default);
            if (placementFile is not null)
            {
                ProcessPlacementFile(builder, featureDescriptor, placementFile);
            }
        }

        return Task.CompletedTask;
    }

    private void ProcessPlacementFile(ShapeTableBuilder builder, IFeatureInfo featureDescriptor, PlacementFile placementFile)
    {
        foreach (var entry in placementFile)
        {
            var shapeType = entry.Key;

            foreach (var filter in entry.Value)
            {
                var matches = filter.Filters.ToList();

                Func<ShapePlacementContext, bool> predicate = ctx => CheckFilter(ctx, filter);

                if (matches.Count > 0)
                {
                    predicate = matches.Aggregate(predicate, BuildPredicate);
                }

                var placement = new PlacementInfo
                {
                    Location = filter.Location,
                };

                if (filter.Alternates?.Length > 0)
                {
                    placement.Alternates = new AlternatesCollection(filter.Alternates);
                }

                if (filter.Wrappers?.Length > 0)
                {
                    placement.Wrappers = new AlternatesCollection(filter.Wrappers);
                }

                placement.ShapeType = filter.ShapeType;

                builder.Describe(shapeType)
                    .From(featureDescriptor)
                    .Placement(ctx => predicate(ctx), placement);
            }
        }
    }

    public static bool CheckFilter(ShapePlacementContext ctx, PlacementNode filter)
    {
        if (!string.IsNullOrEmpty(filter.DisplayType) && filter.DisplayType != ctx.DisplayType)
        {
            return false;
        }

        if (!string.IsNullOrEmpty(filter.Differentiator) && filter.Differentiator != ctx.Differentiator)
        {
            return false;
        }

        return true;
    }

    private Func<ShapePlacementContext, bool> BuildPredicate(Func<ShapePlacementContext, bool> predicate,
          KeyValuePair<string, object> term)
    {
        return BuildPredicate(predicate, term, _placementParseMatchProviders);
    }

    public static Func<ShapePlacementContext, bool> BuildPredicate(Func<ShapePlacementContext, bool> predicate,
            KeyValuePair<string, object> term, IEnumerable<IPlacementNodeFilterProvider> placementMatchProviders)
    {
        if (placementMatchProviders != null)
        {
            var providersForTerm = placementMatchProviders.Where(x => x.Key.Equals(term.Key, StringComparison.Ordinal));
            if (providersForTerm.Any())
            {
                var expression = term.Value;
                return ctx => providersForTerm.Any(x => x.IsMatch(ctx, expression)) && predicate(ctx);
            }
        }
        return predicate;
    }
}
