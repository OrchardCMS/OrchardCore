using System;
using System.Collections.Generic;
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
        string TagName { get; set; }
        IList<string> Classes { get; }
        IDictionary<string, string> Attributes { get; }
        IDictionary<string, object> Properties { get; }
    }

    public static class IShapeExtensions
    {
        private static readonly JsonSerializer ShapeSerializer = new JsonSerializer
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        public static JObject ShapeToJson(this IShape shape)
        {
            if (shape == null)
            {
                return new JObject();
            }

            var jObject = new JObject();

            // Provides a convenient identifier in console.
            var displayText = shape.Metadata.Name;
            if (String.IsNullOrEmpty(displayText))
            {
                displayText = shape.Metadata.Type;
            }

            if (String.IsNullOrEmpty(displayText))
            {
                displayText = shape.GetType().Name;
            }

            jObject.Add("Shape", displayText);

            var metadata = JObject.FromObject(shape.Metadata, ShapeSerializer);

            jObject.Add(nameof(ShapeMetadata), metadata);

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
                FindShapesInProperties(shape);
            }

            var actualShape = shape as Shape;
            if (actualShape != null && actualShape.HasItems)
            {
                var jItems = new JArray();
                // Because items can be mutated during shape execution.
                var shapeItems = actualShape.Items.ToArray();
                foreach (IShape item in shapeItems)
                {
                    // Display item in json so source of item remains clear.
                    var jItem = item.ShapeToJson();
                    jItems.Add(jItem);
                }
                jObject.Add(nameof(actualShape.Items), jItems);
            }

            return jObject;
        }

        private static void FindShapesInProperties(IShape shape)
        {
            foreach (var property in shape.Properties.Values)
            {
                var shapeProperty = property as Shape;
                if (shapeProperty != null && shapeProperty.HasItems)
                {
                    var shapeItems = shapeProperty.Items.ToArray();
                    foreach (IShape item in shapeItems)
                    {
                        // Recurse for more shapes.
                        FindShapesInProperties(item);
                    }
                }
            }
        }
    }
}
