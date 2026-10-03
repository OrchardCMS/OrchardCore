using Fluid.Accessors;
using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.DisplayManagement.Liquid;

internal sealed class ShapeAccessor : DelegateAccessor<object, object>
{
    public ShapeAccessor() : base((obj, name, ctx) => _getter(obj, name))
    {
    }

    private static Func<object, string, object> _getter => (o, n) =>
    {
        if (o is Shape shape)
        {
            object obj = n switch
            {
                nameof(Shape.Id) => shape.Id,
                nameof(Shape.TagName) => shape.TagName,
                nameof(Shape.HasItems) => shape.HasItems,
                nameof(Shape.Classes) => shape.Classes,
                nameof(Shape.Attributes) => shape.Attributes,
                nameof(Shape.Metadata) => shape.Metadata,
                nameof(Shape.Items) => shape.Items,
                nameof(Shape.Properties) => shape.Properties,
                _ => null
            };

            if (obj != null)
            {
                return obj;
            }

            if (shape.Properties.TryGetValue(n, out obj))
            {
                return obj;
            }

            // 'MyType-MyField-FieldType_Display__DisplayMode'.
            var namedShaped = shape.Named(n);
            if (namedShaped != null)
            {
                return namedShaped;
            }

            // 'MyNamedPart', 'MyType__MyField' 'MyType-MyField'.
            return shape.NormalizedNamed(n.Replace("__", "-"));
        }

        return null;
    };
}
