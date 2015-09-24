using Orchard.Events;
using Orchard.DependencyInjection;

namespace Orchard.DisplayManagement.Descriptors {

    public interface IShapeTableManager : ISingletonDependency {
        ShapeTable GetShapeTable(string themeName);
    }

    public interface IShapeTableProvider : IDependency {
        void Discover(ShapeTableBuilder builder);
    }

    public interface IShapeTableEventHandler : IEventHandler {
        void ShapeTableCreated(ShapeTable shapeTable);
    }

    public class Test : IShapeTableProvider {
        public void Discover(ShapeTableBuilder builder) {
            builder.Describe("Foo")
               .OnDisplaying(displaying => displaying.Shape.ChildContent = "<h1>Hi</h1>");
        }
    }
}
