using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;

namespace OrchardCore.DisplayManagement.Descriptors.ShapePlacementStrategy
{
    /// <summary>
    /// This component discovers and announces the shape alterations implied by the contents of the Placement.info files
    /// </summary>
    public class ShapePlacementParsingStrategy : IShapeTableHarvester
    {
        private readonly IHostingEnvironment _hostingEnviroment;
        private readonly IShellFeaturesManager _shellFeaturesManager;
        private readonly ILogger _logger;

        public ShapePlacementParsingStrategy(
            IHostingEnvironment hostingEnviroment,
            IShellFeaturesManager shellFeaturesManager,
            ILogger<ShapePlacementParsingStrategy> logger)
        {
            _logger = logger;
            _hostingEnviroment = hostingEnviroment;
            _shellFeaturesManager = shellFeaturesManager;
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
            var virtualFileInfo = _hostingEnviroment
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
                var matches = entry.Value;


                foreach (var filter in entry.Value)
                {
                    var placement = new PlacementInfo();
                    placement.Location = filter.Location;
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
                        .Placement(ctx => CheckFilter(ctx, filter), placement);
                }
            }
        }

        public static bool CheckFilter(ShapePlacementContext ctx, PlacementNode filter)
        {
            if (!String.IsNullOrEmpty(filter.DisplayType) && filter.DisplayType != ctx.DisplayType)
            {
                return false;
            }

            if (!String.IsNullOrEmpty(filter.Differentiator) && filter.Differentiator != ctx.Differentiator)
            {
                return false;
            }

            return true;

            //switch (term.Key)
            //{
            //case "ContentPart":
            //    return ctx => ctx.Content != null
            //        && ctx.Content.ContentItem.Has(expression)
            //        && predicate(ctx);
            //case "ContentType":
            //    if (expression.EndsWith("*"))
            //    {
            //        var prefix = expression.Substring(0, expression.Length - 1);
            //        return ctx => ((ctx.ContentType ?? "").StartsWith(prefix) || (ctx.Stereotype ?? "").StartsWith(prefix)) && predicate(ctx);
            //    }
            //    return ctx => ((ctx.ContentType == expression) || (ctx.Stereotype == expression)) && predicate(ctx);
            //case "DisplayType":
            //    if (expression.EndsWith("*"))
            //    {
            //        var prefix = expression.Substring(0, expression.Length - 1);
            //        return ctx => (ctx.DisplayType ?? "").StartsWith(prefix) && predicate(ctx);
            //    }
            //    return ctx => (ctx.DisplayType == expression) && predicate(ctx);
            //case "Path":
            //    throw new Exception("Path Not currently Supported");
            //var normalizedPath = VirtualPathUtility.IsAbsolute(expression)
            //                         ? VirtualPathUtility.ToAppRelative(expression)
            //                         : VirtualPathUtility.Combine("~/", expression);

            //if (normalizedPath.EndsWith("*")) {
            //    var prefix = normalizedPath.Substring(0, normalizedPath.Length - 1);
            //    return ctx => VirtualPathUtility.ToAppRelative(String.IsNullOrEmpty(ctx.Path) ? "/" : ctx.Path).StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && predicate(ctx);
            //}

            //normalizedPath = VirtualPathUtility.AppendTrailingSlash(normalizedPath);
            //return ctx => (ctx.Path.Equals(normalizedPath, StringComparison.OrdinalIgnoreCase)) && predicate(ctx);
            //}
        }
    }
}