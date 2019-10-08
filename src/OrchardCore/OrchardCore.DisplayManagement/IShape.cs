using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.DisplayManagement
{
    /// <summary>
    /// Interface present on dynamic shapes.
    /// May be used to access attributes in a strongly typed fashion.
    /// Note: Anything on this interface is a reserved word for the purpose of shape properties
    /// </summary>
    public interface IShape
    {
        ShapeMetadata Metadata { get; }
        string Id { get; set; }
        IList<string> Classes { get; }
        IDictionary<string, string> Attributes { get; }
        IDictionary<string, object> Properties { get; }
    }


    public static class IShapeExtensions
    {
        public static readonly JsonSerializer ShapeSerializer = new JsonSerializer
        {
            DefaultValueHandling = DefaultValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        public static JObject ShapeDump(this IShape shape)
        {
            if (shape == null)
            {
                return new JObject();
            }

            var jObject = new JObject();
            var metadata = JObject.FromObject(shape.Metadata, ShapeSerializer);
            if (metadata.HasValues)
            {
                jObject.Add(nameof(ShapeMetadata), metadata);
            }

            if (shape.Classes != null && shape.Classes.Any())
            {
                jObject.Add(nameof(shape.Classes), JArray.FromObject(shape.Classes, ShapeSerializer));
            }
            if (shape.Attributes != null && shape.Attributes.Any())
            {
                jObject.Add(nameof(shape.Attributes), JObject.FromObject(shape.Attributes, ShapeSerializer));
            }
            if (shape.Properties != null && shape.Properties.Any())
            {
                jObject.Add(nameof(shape.Properties), JObject.FromObject(shape.Properties, ShapeSerializer));
            }

            var actualShape = shape as Shape;
            if (actualShape != null && actualShape.HasItems)
            {
                var items = new JArray();
                // Because items can be mutated during shape execution.
                var shapeItems = actualShape.Items.ToImmutableArray();
                foreach (IShape item in shapeItems)
                {
                    var itemResult = item.ShapeDump();
                    if (itemResult.HasValues)
                    {
                        items.Add(itemResult);
                    }
                }
                if (items.HasValues)
                {
                    jObject.Add(nameof(actualShape.Items), items);
                }
            }

            return jObject;
        }
    }
}
