using Orchard.Environment.Extensions.Models;

namespace Orchard.DisplayManagement.Descriptors {
    public class FooShapeProvider : IShapeTableProvider {
        public void Discover(ShapeTableBuilder builder) {
            builder.Describe("Foo")
               .OnDisplaying(displaying => displaying.Shape.ChildContent = "<h1>Hi</h1>");
        }

        public Feature Feature {
            get {
                return new Feature {
                    Descriptor = new FeatureDescriptor {
                        Id = "Orchard.Demo",
                        Extension = new ExtensionDescriptor {
                            Id = "Orchard.Demo",
                            ExtensionType = DefaultExtensionTypes.Module
                        }
                    },
                };
            }
        }
    }
}
