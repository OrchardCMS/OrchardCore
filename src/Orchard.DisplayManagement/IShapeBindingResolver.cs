using Orchard.DisplayManagement.Descriptors;
using Orchard.DependencyInjection;

namespace Orchard.DisplayManagement
{
    public interface IShapeBindingResolver : IDependency
    {
        bool TryGetDescriptorBinding(string shapeType, out ShapeBinding shapeBinding);
    }
}