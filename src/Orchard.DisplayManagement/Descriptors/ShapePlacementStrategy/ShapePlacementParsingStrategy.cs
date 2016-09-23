using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Orchard.Environment.Extensions.Features;
using Orchard.Environment.Extensions.FileSystem;
using Orchard.Environment.Extensions.Models;
using Orchard.FileSystem;

namespace Orchard.DisplayManagement.Descriptors.ShapePlacementStrategy
{
    /// <summary>
    /// This component discovers and announces the shape alterations implied by the contents of the Placement.info files
    /// </summary>
    public class ShapePlacementParsingStrategy : IShapeTableProvider
    {
        private readonly IFeatureManager _featureManager;
        private readonly IOrchardFileSystem _fileSystem;
        private readonly ILogger _logger;

        public ShapePlacementParsingStrategy(
            IFeatureManager featureManager,
            IOrchardFileSystem fileSystem,
            ILogger<ShapePlacementParsingStrategy> logger)
        {
            _logger = logger;
            _featureManager = featureManager;
            _fileSystem = fileSystem;
        }

        public void Discover(ShapeTableBuilder builder)
        {
            foreach (var featureDescriptor in _featureManager.GetEnabledFeaturesAsync().Result)
            {
                ProcessFeatureDescriptor(builder, featureDescriptor);
            }

        }

        private void ProcessFeatureDescriptor(ShapeTableBuilder builder, FeatureDescriptor featureDescriptor)
        {
            var virtualPath = _fileSystem
                .GetExtensionFileProvider(featureDescriptor.Extension, _logger)
                .GetFileInfo("placement.json");

            if (virtualPath.Exists)
            {
                using (var stream = virtualPath.CreateReadStream())
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

        private void ProcessPlacementFile(ShapeTableBuilder builder, FeatureDescriptor featureDescriptor, PlacementFile placementFile)
        {
            var feature = new Feature { Descriptor = featureDescriptor };

            foreach (var entry in placementFile)
            {
                var shapeType = entry.Key;
                var matches = entry.Value;


                foreach (var filter in entry.Value)
                {
                    var placement = new PlacementInfo();
                    placement.Location = filter.Location;
                    placement.Alternates = filter.Alternates;
                    placement.Wrappers = filter.Wrappers;
                    placement.ShapeType = filter.ShapeType;

                    builder.Describe(shapeType)
                        .From(feature)
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

        private bool FeatureIsTheme(FeatureDescriptor fd)
        {
            return DefaultExtensionTypes.IsTheme(fd.Extension.ExtensionType);
        }
    }
}
