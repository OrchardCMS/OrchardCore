using Orchard.DisplayManagement.Descriptors;

namespace Orchard.DisplayManagement
{
    /// <summary>
    /// TODO: Document
    /// There are no implementation, clarify the usage.
    /// </summary>
    public interface IShapeBindingResolver
    {
        bool TryGetDescriptorBinding(string shapeType, out ShapeBinding shapeBinding);
    }
}