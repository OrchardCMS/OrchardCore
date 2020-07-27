using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Zones;

namespace OrchardCore.DisplayManagement
{
    public abstract class BaseDisplayManager
    {
        private readonly IShapeFactory _shapeFactory;
        private readonly IEnumerable<IShapePlacementProvider> _placementProviders;

        public BaseDisplayManager(
            IShapeFactory shapeFactory,
            IEnumerable<IShapePlacementProvider> placementProviders
            )
        {
            _shapeFactory = shapeFactory;
            _placementProviders = placementProviders;
        }

        protected async Task BindPlacementAsync(IBuildShapeContext context)
        {
            var resolvers = new List<IPlacementInfoResolver>();

            foreach(var provider in _placementProviders)
            {
                var resolver = await provider.BuildPlacementInfoResolverAsync(context);

                if (resolver != null)
                {
                    resolvers.Add(resolver);
                }
            }

            context.FindPlacement = (shapeType, differentiator, displayType) => FindPlacementImpl(resolvers, shapeType, differentiator, displayType, context);
        }

        private static PlacementInfo FindPlacementImpl(IList<IPlacementInfoResolver> placementResolvers, string shapeType, string differentiator, string displayType, IBuildShapeContext context)
        {
            var delimiterIndex = shapeType.IndexOf("__", StringComparison.Ordinal);

            if (delimiterIndex > 0)
            {
                shapeType = shapeType.Substring(0, delimiterIndex);
            }

            var placementContext = new ShapePlacementContext(
                shapeType,
                displayType,
                differentiator,
                context.Shape
            );

            for(int i = placementResolvers.Count - 1; i >= 0; i--)
            {
                var info = placementResolvers[i].ResolvePlacement(placementContext);

                if (info != null)
                {
                    return info;
                }
            }

            return null;
        }

        protected ValueTask<IShape> CreateContentShapeAsync(string actualShapeType)
        {
            return _shapeFactory.CreateAsync(actualShapeType, () =>
                new ValueTask<IShape>(new ZoneHolding(() => _shapeFactory.CreateAsync("ContentZone"))));
        }
    }
}
