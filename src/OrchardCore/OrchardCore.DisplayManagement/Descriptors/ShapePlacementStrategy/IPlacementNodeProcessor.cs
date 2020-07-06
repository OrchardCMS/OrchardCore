using OrchardCore.Environment.Extensions.Features;

namespace OrchardCore.DisplayManagement.Descriptors.ShapePlacementStrategy
{
    public interface IPlacementNodeProcessor
    {
        void ProcessPlacementNode(ShapeTableBuilder builder, IFeatureInfo featureDescriptor, string shapeType, PlacementNode placementNode);
    }
}
