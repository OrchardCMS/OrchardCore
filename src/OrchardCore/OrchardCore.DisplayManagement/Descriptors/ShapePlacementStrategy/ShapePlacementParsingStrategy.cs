using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
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
        private readonly IPlacementNodeProcessor _placementNodeProcessor;

        public ShapePlacementParsingStrategy(
            IHostEnvironment hostingEnvironment,
            IShellFeaturesManager shellFeaturesManager,
            IPlacementNodeProcessor placementNodeProcessor)
        {
            _hostingEnvironment = hostingEnvironment;
            _shellFeaturesManager = shellFeaturesManager;
            _placementNodeProcessor = placementNodeProcessor;
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

            if (virtualFileInfo.Exists)
            {
                using (var stream = virtualFileInfo.CreateReadStream())
                {
                    using (var reader = new StreamReader(stream))
                    {
                        using (var jtr = new JsonTextReader(reader))
                        {
                            JsonSerializer serializer = new JsonSerializer();
                            var placementFile = serializer.Deserialize<PlacementFile>(jtr);
                            ProcessPlacementFile(builder, featureDescriptor, placementFile);
                        }
                    }
                }
            }
        }

        private void ProcessPlacementFile(ShapeTableBuilder builder, IFeatureInfo featureDescriptor, PlacementFile placementFile)
        {
            foreach (var entry in placementFile)
            {
                var shapeType = entry.Key;

                foreach (var node in entry.Value)
                {
                    _placementNodeProcessor.ProcessPlacementNode(builder, featureDescriptor, shapeType, node);
                }
            }
        }
    }
}
