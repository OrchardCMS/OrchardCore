namespace Orchard.DisplayManagement.Descriptors {
    public class FooShapeProvider : IShapeTableProvider {
        public void Discover(ShapeTableBuilder builder) {
            builder.Describe("Foo")
               .OnDisplaying(displaying => 
               displaying.Shape.ChildContent = "<h1>Hi</h1>");
        }
    }
}
