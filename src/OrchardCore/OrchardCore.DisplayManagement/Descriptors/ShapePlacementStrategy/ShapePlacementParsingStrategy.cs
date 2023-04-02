using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;

namespace OrchardCore.DisplayManagement.Descriptors.ShapePlacementStrategy
{
    /// <summary>
    /// This component discovers and announces the shape alterations implied by the contents of the Placement.json files
    /// </summary>
    public class ShapePlacementParsingStrategy : IShapeTableHarvester
    {
        private readonly IHostEnvironment _hostingEnvironment;
        private readonly IShellFeaturesManager _shellFeaturesManager;
        private readonly IEnumerable<IPlacementNodeFilterProvider> _placementNodeFilterProviders;

        public ShapePlacementParsingStrategy(
            IHostEnvironment hostingEnvironment,
            IShellFeaturesManager shellFeaturesManager,
            IEnumerable<IPlacementNodeFilterProvider> placementNodeFilterProviders)
        {
            _hostingEnvironment = hostingEnvironment;
            _shellFeaturesManager = shellFeaturesManager;
            _placementNodeFilterProviders = placementNodeFilterProviders;
        }

        public void Discover(ShapeTableBuilder builder)
        {
            var enabledFeatures = _shellFeaturesManager.GetEnabledFeaturesAsync().GetAwaiter().GetResult()
                .Where(Feature => !builder.ExcludedFeatureIds.Contains(Feature.Id));

            foreach (var featureDescriptor in enabledFeatures)
            {
                ProcessFeatureDescriptor(builder, featureDescriptor);
            }
        }

        private void ProcessFeatureDescriptor(ShapeTableBuilder builder, IFeatureInfo featureDescriptor)
        {
            // TODO : (ngm) Replace with configuration Provider and read from that.
            // Dont use JSON Deserializer directly.
            var virtualFileInfo = _hostingEnvironment
                .GetExtensionFileInfo(featureDescriptor.Extension, "placement.json");

            if (!virtualFileInfo.Exists)
            {
                return;
            }

            using var stream = virtualFileInfo.CreateReadStream();
            using var reader = new StreamReader(stream);
            using var jtr = new JsonTextReader(reader);

            var serializer = new JsonSerializer();
            var placementFile = serializer.Deserialize<PlacementFile>(jtr);
            if (placementFile != null)
            {
                ProcessPlacementFile(builder, featureDescriptor, placementFile);
            }
        }

        private void ProcessPlacementFile(ShapeTableBuilder builder, IFeatureInfo featureDescriptor, PlacementFile placementFile)
        {
            foreach (var entry in placementFile)
            {
                var shapeType = entry.Key;

                foreach (var filter in entry.Value)
                {
                    var placement = new PlacementInfo
                    {
                        Location = filter.Location,
                        ShapeType = filter.ShapeType,
                        Source = featureDescriptor.Id,
                    };

                    if (filter.Alternates?.Length > 0)
                    {
                        placement.Alternates = new AlternatesCollection(filter.Alternates);
                    }

                    if (filter.Wrappers?.Length > 0)
                    {
                        placement.Wrappers = new AlternatesCollection(filter.Wrappers);
                    }

                    builder.Describe(shapeType)
                        .From(featureDescriptor)
                        .Placement(context => PlacementHelper.MatchesAllFilters(context, filter, _placementNodeFilterProviders), placement);
                }
            }
        }
    }
}
