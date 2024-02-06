using System.Text.Json.Nodes;

namespace OrchardCore.DisplayManagement.Descriptors.ShapePlacementStrategy
{
    public interface IPlacementNodeFilterProvider
    {
        string Key { get; }
        bool IsMatch(ShapePlacementContext context, object expression);
    }
}
