using Orchard.Events;
using Orchard.DependencyInjection;
using System.Threading.Tasks;

namespace Orchard.DisplayManagement.Descriptors
{
    public interface IShapeTableManager : ITransientDependency
    {
        ShapeTable GetShapeTable(string themeName);
    }

    public interface IShapeTableProvider : IDependency
    {
        void Discover(ShapeTableBuilder builder);
    }

    public interface IShapeTableEventHandler : IEventHandler
    {
        void ShapeTableCreated(ShapeTable shapeTable);
    }
}